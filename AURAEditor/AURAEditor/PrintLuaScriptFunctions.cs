using System.Text;
using Windows.UI.Xaml.Controls;
using static AuraEditor.Common.LuaHelper;
using static AuraEditor.Common.EffectHelper;
using MoonSharp.Interpreter;
using System.Collections.Generic;

namespace AuraEditor
{
    public sealed partial class MainPage : Page
    {
        public string PrintLuaScript()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(RequireLine);
            sb.Append("\n\n");
            sb.Append("EventProvider = ");
            sb.Append(PrintTable(GetEventProviderTable()));
            sb.Append("\n");
            sb.Append("Viewport = ");
            sb.Append(PrintTable(GetViewportTable()));
            sb.Append("\n");
            sb.Append("Event = ");
            sb.Append(PrintTable(GetEventTable()));
            sb.Append("\n");
            sb.Append("GlobalSpace = ");
            sb.Append(PrintTable(GetGlobalSpaceTable()));
            sb.Append("\n");
            return sb.ToString();
        }
        private Table GetEventProviderTable()
        {
            Table eventProviderTable = CreateNewTable();
            Table queueTable = GetQueueTable();
            DynValue generateEventDV = RegisterAndGetDV(GenerateEventFunctionString);

            eventProviderTable.Set("queue", DynValue.NewTable(queueTable));
            eventProviderTable.Set("period", DynValue.NewNumber(LayerManager.PlayTime));
            eventProviderTable.Set("generateEvent", generateEventDV);

            return eventProviderTable;
        }

        public double GetPlayTime()
        {
            return LayerManager.PlayTime;
        }
        public double GetRightmostPosition()
        {
            return LayerManager.RightmostPosition;
        }

        private Table GetQueueTable()
        {
            Table queueTable;
            Table queueItemTable;
            int effectCount;
            int queueIndex;

            queueTable = CreateNewTable();
            effectCount = 0;
            queueIndex = 1;

            foreach (DeviceLayer layer in LayerManager.DeviceLayerCollection)
            {
                if (layer.Eye == false)
                    continue;

                List<Effect> effCollection = new List<Effect>();
                effCollection.AddRange(layer.TimelineEffects);
                effCollection.AddRange(layer.TriggerEffects);

                foreach (Effect eff in effCollection)
                {
                    // Give uniqle index for all effects
                    eff.LuaName = GetEffectName(eff.Type) + effectCount.ToString();
                    effectCount++;

                    queueItemTable = CreateNewTable();
                    queueItemTable.Set("Effect", DynValue.NewString(eff.LuaName));
                    queueItemTable.Set("Viewport", DynValue.NewString(layer.Name));

                    if (IsTriggerEffects(eff.Type))
                        queueItemTable.Set("Trigger", DynValue.NewString("KeyboardInput"));
                    else
                        queueItemTable.Set("Trigger", DynValue.NewString("Period"));

                    queueItemTable.Set("Delay", DynValue.NewNumber(eff.StartTime));
                    queueItemTable.Set("Duration", DynValue.NewNumber(eff.DurationTime));
                    queueTable.Set(queueIndex, DynValue.NewTable(queueItemTable));
                    queueIndex++;
                }
            }

            return queueTable;
        }
        private Table GetViewportTable()
        {
            Table viewPortTable;
            Table layerTable;

            viewPortTable = CreateNewTable();

            foreach (DeviceLayer layer in LayerManager.DeviceLayerCollection)
            {
                layerTable = layer.ToTable();
                viewPortTable.Set(layer.Name, DynValue.NewTable(layerTable));
            }

            return viewPortTable;
        }
        private Table GetEventTable()
        {
            Table eventTable = CreateNewTable();

            foreach (DeviceLayer layer in LayerManager.DeviceLayerCollection)
            {
                if (layer.Eye == false)
                    continue;

                List<Effect> effCollection = new List<Effect>();
                effCollection.AddRange(layer.TimelineEffects);
                effCollection.AddRange(layer.TriggerEffects);

                foreach (Effect eff in effCollection)
                    eventTable.Set(eff.LuaName, DynValue.NewTable(eff.ToTable()));
            }

            return eventTable;
        }

        public void RescanIngroupDevices()
        {
            SpaceManager.RescanIngroupDevices();
        }

        private Table GetGlobalSpaceTable()
        {
            Table globalSpaceTable = CreateNewTable();

            foreach (Device d in SpaceManager.GlobalDevices)
                globalSpaceTable.Set(d.Name, DynValue.NewTable(d.ToTable()));

            return globalSpaceTable;
        }
        private string PrintTable(Table tb, int tab = 0)
        {
            StringBuilder sb = new StringBuilder();

            string paranthesesTabString = "";
            string otherTabString = "";

            for (int i = 0; i < tab; i++)
            {
                paranthesesTabString += "\t";
            }
            otherTabString = paranthesesTabString + "\t";

            sb.Append(paranthesesTabString + "{\n");
            foreach (var key in tb.Keys)
            {
                DynValue keyDV;
                string keyName = "";
                string keyValue = "";

                // key name
                if (key.Type.ToString() == "String")
                {
                    keyName = "[\"" + key.String + "\"]";
                    keyDV = tb.Get(key.String);
                }
                else
                {
                    keyName = "[" + key.Number.ToString() + "]";
                    keyDV = tb.Get(key.Number);
                }

                // key value
                if (keyDV.Table != null)
                {
                    keyValue = PrintTable(keyDV.Table, tab + 1);
                    sb.Append(otherTabString + keyName + " =\n" + keyValue + ",\n");
                }
                else
                {
                    if (keyDV.String != null)
                    {
                        keyValue = "\"" + keyDV.String + "\"";
                    }
                    else if (keyDV.Function != null)
                    {
                        keyValue = GetFunctionString(keyDV);
                    }
                    else
                    {
                        keyValue = keyDV.Number.ToString();
                    }
                    sb.Append(otherTabString + keyName + " = " + keyValue + ",\n");
                }
            }

            sb.Append(paranthesesTabString + "}");
            return sb.ToString();
        }
    }
}