using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using static AuraEditor.Common.LuaHelper;
using static AuraEditor.Common.EffectHelper;
using MoonSharp.Interpreter;
using System.Xml;

namespace AuraEditor
{
    public class UIInfo
    {
        public string Name;
        public int Type;
        public Color InitColor;
        public int Brightness;
        public int Speed;
        public double Angle;
        public int Direction;
        public bool Random;
        public int High;
        public int Low;

        public UIInfo()
        {
        }

        public UIInfo(int type)
        {
            Name = GetEffectName(type);
            Type = type;
            InitColor = Colors.Red;
            Brightness = 3;
            Speed = 1;
            Direction = 2;
            Angle = 90;
            Random = false;
            High = 60;
            Low = 30;
        }

        public XmlNode ToXmlNode()
        {
            XmlNode effect = CreateXmlNodeOfFile("effect");

            XmlNode type = CreateXmlNodeOfFile("type");
            type.InnerText = Type.ToString();
            effect.AppendChild(type);

            XmlNode a = CreateXmlNodeOfFile("a");
            a.InnerText = InitColor.A.ToString();
            effect.AppendChild(a);

            XmlNode r = CreateXmlNodeOfFile("r");
            r.InnerText = InitColor.R.ToString();
            effect.AppendChild(r);

            XmlNode g = CreateXmlNodeOfFile("g");
            g.InnerText = InitColor.G.ToString();
            effect.AppendChild(g);

            XmlNode b = CreateXmlNodeOfFile("b");
            b.InnerText = InitColor.B.ToString();
            effect.AppendChild(b);

            XmlNode brightness = CreateXmlNodeOfFile("brightness");
            brightness.InnerText = Brightness.ToString();
            effect.AppendChild(brightness);

            XmlNode speed = CreateXmlNodeOfFile("speed");
            speed.InnerText = Speed.ToString();
            effect.AppendChild(speed);

            XmlNode angle = CreateXmlNodeOfFile("angle");
            angle.InnerText = Angle.ToString();
            effect.AppendChild(angle);

            XmlNode direction = CreateXmlNodeOfFile("direction");
            direction.InnerText = Direction.ToString();
            effect.AppendChild(direction);

            XmlNode random = CreateXmlNodeOfFile("random");
            random.InnerText = Random.ToString();
            effect.AppendChild(random);

            XmlNode high = CreateXmlNodeOfFile("high");
            high.InnerText = High.ToString();
            effect.AppendChild(high);

            XmlNode low = CreateXmlNodeOfFile("low");
            low.InnerText = Low.ToString();
            effect.AppendChild(low);

            return effect;
        }
    }
}
