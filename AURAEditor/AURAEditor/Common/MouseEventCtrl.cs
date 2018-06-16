using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace AuraEditor.Common
{
    static class RegionStatus
    {
        public const int Normal = 0;
        public const int NormalHover = 1;
        public const int Selected = 2;
        public const int SelectedHover = 3;
    }

    class Region
    {
        bool hover;
        bool selected;

        //public Region(int i, Rect r, MouseEventCtrl.StatusChangedCallBack cb)
        //{
        //    Index = i;
        //    MyRect = r;
        //    hover = false;
        //    selected = false;
        //    this.cb = cb;
        //}

        public MouseEventCtrl.StatusChangedCallBack Callback { get; set; }

        public Rect MyRect { get; set; }

        public int Index { get; set; }

        public bool IsHover => hover;

        public bool Hover
        {
            set
            {
                if (hover != value)
                {
                    hover = value;
                    OnStatusChanged();
                }
            }
        }

        public bool IsSelected => selected;

        public bool Selected
        {
            set
            {
                if (selected != value)
                {
                    selected = value;
                    OnStatusChanged();
                }
            }
        }

        public void OnStatusChanged()
        {
            int status;

            if (!hover && !selected) status = RegionStatus.Normal;
            else if (hover && !selected) status = RegionStatus.NormalHover;
            else if (!hover && selected) status = RegionStatus.Selected;
            else status = RegionStatus.SelectedHover;

            Callback?.Invoke(Index, status);
        }
    }

    class MouseEventCtrl
    {
        Region[] _regions;
        Rect mouseRect;
        Point pressPoint;
        List<int> beforeMouseSelectedIndexes;
        List<int> currentMouseSelectedIndexes;
        int currentHoverIndex;

        private Rect monitorMaxRect;
        public Rect MonitorMaxRect { get => monitorMaxRect; set => monitorMaxRect = value; }

        public delegate void StatusChangedCallBack(int regionIndex, int status);

        public Region[] MyRegions
        {
            get => _regions;
            set { _regions = value; }
        }

        public int CurrentHoverIndex
        {
            get => currentHoverIndex;
            set
            {
                if (currentHoverIndex != value)
                {
                    if (currentHoverIndex != -1)
                        MyRegions[currentHoverIndex].Hover = false;
                    if (value != -1)
                        MyRegions[value].Hover = true;
                    currentHoverIndex = value;
                }
            }
        }

        public MouseEventCtrl()
        {
            beforeMouseSelectedIndexes = new List<int>();
            currentMouseSelectedIndexes = new List<int>();
        }

        public void OnMousePressed(Point p)
        {
            for (int i = 0; i < MyRegions.Length; i++)
            {
                if (MyRegions[i].MyRect.Contains(p))
                {
                    MyRegions[i].Selected = !MyRegions[i].IsSelected;
                }
            }

            pressPoint.X = p.X;
            pressPoint.Y = p.Y;
        }

        public void OnMouseMoved(Point p, bool isLeftButtonPressed)
        {
            if (isLeftButtonPressed == false)
            {
                for (int i = 0; i < MyRegions.Length; i++)
                {
                    if (MyRegions[i].MyRect.Contains(p))
                    {
                        CurrentHoverIndex = i;
                        return; // just check which region is hovering
                    }
                }
                CurrentHoverIndex = -1;
                return; // just check which region is hovering
            }

            // -1 mean pressPoint is out of bounds currently, we should give new pressPoint
            if (pressPoint.X == -1)
            {
                pressPoint.X = p.X;
                pressPoint.Y = p.Y;
            }

            for (int i = 0; i < MyRegions.Length; i++)
            {
                if (MyRegions[i].MyRect.Contains(pressPoint))
                {
                    mouseRect.Width = 0;
                    mouseRect.Height = 0;
                    return; // No need to update mouse rectangle if pressPoint in one of region
                }
            }

            // Mouse rectangle
            double current_x = p.X;
            double current_y = p.Y;
            int maxWidth = (int)MonitorMaxRect.Width;
            int maxHeight = (int)MonitorMaxRect.Height;

            // point is out of bounds?
            if (current_x < 0) current_x = 0;
            else if (current_x > maxWidth) current_x = maxWidth;
            if (current_y < 0) current_y = 0;
            else if (current_y > maxHeight) current_y = maxHeight;

            double x = Math.Min(current_x, pressPoint.X);
            double y = Math.Min(current_y, pressPoint.Y);

            mouseRect.Width = Math.Max(current_x, pressPoint.X) - Math.Min(current_x, pressPoint.X);
            mouseRect.Height = Math.Max(current_y, pressPoint.Y) - Math.Min(current_y, pressPoint.Y);
            mouseRect.X = x;
            mouseRect.Y = y;

            // Detect intersection
            currentMouseSelectedIndexes.Clear();
            for (int i = 0; i < MyRegions.Length; i++)
            {
                Rect r = MyRegions[i].MyRect;
                r.Intersect(mouseRect);

                if (r != Rect.Empty)
                {
                    currentMouseSelectedIndexes.Add(i);
                }
            }

            // Get outside and inside region index, and then toggle its Selected
            int[] outsideIndexes = beforeMouseSelectedIndexes.Except(currentMouseSelectedIndexes).ToArray();
            int[] insideIndexes = currentMouseSelectedIndexes.Except(beforeMouseSelectedIndexes).ToArray();

            for (int i = 0; i < outsideIndexes.Length; i++)
            {
                int index = outsideIndexes[i];
                MyRegions[index].Selected = !MyRegions[index].IsSelected;
            }
            for (int i = 0; i < insideIndexes.Length; i++)
            {
                int index = insideIndexes[i];
                MyRegions[index].Selected = !MyRegions[index].IsSelected;
            }

            beforeMouseSelectedIndexes = new List<int>(currentMouseSelectedIndexes);
        }

        public void OnMouseReleased()
        {
            currentMouseSelectedIndexes.Clear();
            beforeMouseSelectedIndexes.Clear();
            pressPoint.X = -1;
            pressPoint.Y = -1;
            mouseRect.Width = 0;
            mouseRect.Height = 0;
        }

        public void OnRightTapped()
        {
            for (int i = 0; i < MyRegions.Length; i++)
            {
                MyRegions[i].Selected = false;
            }
        }

        public int[] GetSelectedIndexes()
        {
            List<int> selectedIndex = new List<int>();
            foreach (Region r in MyRegions)
            {
                if (r.IsSelected == true)
                    selectedIndex.Add(r.Index);
            }

            return selectedIndex.ToArray();
        }

        public Rect MouseRect => mouseRect;
    }
}
