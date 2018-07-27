using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace AuraEditor.Common
{
    public enum RegionStatus
    {
        Normal = 0,
        NormalHover,
        Selected,
        SelectedHover
    }

    class MouseDetectionRegion
    {
        public MouseEventCtrl.StatusChangedCallBack Callback { get; set; }
        public Rect DetectionRect { get; set; }
        public int RegionIndex { get; set; }
        public int GroupIndex { get; set; }

        bool hover;
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

        bool selected;
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

        public void UpdateRect(int moveXOffset, int moveYOffset)
        {
            Rect r = new Rect(
                DetectionRect.X + moveXOffset,
                DetectionRect.Y + moveYOffset,
                DetectionRect.Width,
                DetectionRect.Height);

            DetectionRect = r;
        }
        public void OnStatusChanged()
        {
            RegionStatus status;

            if (!hover && !selected) status = RegionStatus.Normal;
            else if (hover && !selected) status = RegionStatus.NormalHover;
            else if (!hover && selected) status = RegionStatus.Selected;
            else status = RegionStatus.SelectedHover;

            Callback?.Invoke(RegionIndex, status);
        }
    }

    class MouseEventCtrl
    {
        public delegate void StatusChangedCallBack(int regionIndex, RegionStatus status);
        Point _pressPoint;
        List<int> _beforeMouseSelectedIndexes;
        List<int> _currentMouseSelectedIndexes;

        private Rect _monitorMaxRect;
        public Rect MonitorMaxRect
        {
            get => _monitorMaxRect;
            set => _monitorMaxRect = value;
        }
        
        private MouseDetectionRegion[] _detectionRegions;
        public MouseDetectionRegion[] DetectionRegions
        {
            get => _detectionRegions;
            set { _detectionRegions = value; }
        }

        private int _currentHoverIndex;
        public int CurrentHoverIndex
        {
            get => _currentHoverIndex;
            set
            {
                if (_currentHoverIndex != value)
                {
                    if (_currentHoverIndex != -1)
                        DetectionRegions[_currentHoverIndex].Hover = false;
                    if (value != -1)
                        DetectionRegions[value].Hover = true;
                    _currentHoverIndex = value;
                }
            }
        }
        
        public Rect MouseRect { get; set; }

        public MouseEventCtrl()
        {
            _pressPoint = new Point(0, 0);
            _monitorMaxRect = new Rect(new Point(0, 0), new Point(0, 0));
            _beforeMouseSelectedIndexes = new List<int>();
            _currentMouseSelectedIndexes = new List<int>();
            _currentHoverIndex = -1;
        }

        public void OnMousePressed(Point p)
        {
            for (int i = 0; i < DetectionRegions.Length; i++)
            {
                if (DetectionRegions[i].DetectionRect.Contains(p))
                {
                    DetectionRegions[i].Selected = !DetectionRegions[i].IsSelected;
                }
            }

            _pressPoint.X = p.X;
            _pressPoint.Y = p.Y;
        }
        public void OnMouseMoved(Point p, bool isLeftButtonPressed)
        {
            Rect mouseRect = MouseRect;

            if (isLeftButtonPressed == false)
            {
                for (int i = 0; i < DetectionRegions.Length; i++)
                {
                    if (DetectionRegions[i].DetectionRect.Contains(p))
                    {
                        CurrentHoverIndex = i;
                        return; // just check which region is hovering
                    }
                }
                CurrentHoverIndex = -1;
                return; // just check which region is hovering
            }

            // -1 mean pressPoint is out of bounds currently, we should give new pressPoint
            if (_pressPoint.X == -1)
            {
                _pressPoint.X = p.X;
                _pressPoint.Y = p.Y;
            }

            for (int i = 0; i < DetectionRegions.Length; i++)
            {
                if (DetectionRegions[i].DetectionRect.Contains(_pressPoint))
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

            double x = Math.Min(current_x, _pressPoint.X);
            double y = Math.Min(current_y, _pressPoint.Y);

            mouseRect.Width = Math.Max(current_x, _pressPoint.X) - Math.Min(current_x, _pressPoint.X);
            mouseRect.Height = Math.Max(current_y, _pressPoint.Y) - Math.Min(current_y, _pressPoint.Y);
            mouseRect.X = x;
            mouseRect.Y = y;

            // Detect intersection
            _currentMouseSelectedIndexes.Clear();
            for (int i = 0; i < DetectionRegions.Length; i++)
            {
                Rect r = DetectionRegions[i].DetectionRect;
                r.Intersect(mouseRect);

                if (r != Rect.Empty)
                {
                    _currentMouseSelectedIndexes.Add(i);
                }
            }

            // Get outside and inside region index, and then toggle its Selected
            int[] outsideIndexes = _beforeMouseSelectedIndexes.Except(_currentMouseSelectedIndexes).ToArray();
            int[] insideIndexes = _currentMouseSelectedIndexes.Except(_beforeMouseSelectedIndexes).ToArray();

            for (int i = 0; i < outsideIndexes.Length; i++)
            {
                int index = outsideIndexes[i];
                DetectionRegions[index].Selected = !DetectionRegions[index].IsSelected;
            }
            for (int i = 0; i < insideIndexes.Length; i++)
            {
                int index = insideIndexes[i];
                DetectionRegions[index].Selected = !DetectionRegions[index].IsSelected;
            }

            _beforeMouseSelectedIndexes = new List<int>(_currentMouseSelectedIndexes);
            MouseRect = mouseRect;
        }
        public void OnMouseReleased()
        {
            Rect mouseRect = MouseRect;

            _currentMouseSelectedIndexes.Clear();
            _beforeMouseSelectedIndexes.Clear();
            _pressPoint.X = -1;
            _pressPoint.Y = -1;
            mouseRect.Width = 0;
            mouseRect.Height = 0;

            MouseRect = mouseRect;
        }
        public void OnRightTapped()
        {
            for (int i = 0; i < DetectionRegions.Length; i++)
            {
                DetectionRegions[i].Selected = false;
            }
        }

        public int[] GetSelectedIndexes()
        {
            List<int> selectedIndex = new List<int>();
            foreach (var r in DetectionRegions)
            {
                if (r.IsSelected == true)
                    selectedIndex.Add(r.RegionIndex);
            }

            return selectedIndex.ToArray();
        }
        public void UpdateGroupRects(int groupIndex, int moveXOffset, int moveYOffset)
        {
            foreach(var reg in DetectionRegions)
            {
                if (reg.GroupIndex == groupIndex)
                {
                    reg.UpdateRect(moveXOffset, moveYOffset);
                }
            }
        }
        public void SetAllRegionsStatus(RegionStatus status)
        {
            bool hover;
            bool selected;

            if (status == RegionStatus.Normal)
            { hover = false; selected = false; }
            else if (status == RegionStatus.NormalHover)
            { hover = true; selected = false; }
            else if (status == RegionStatus.Selected)
            { hover = false; selected = true; }
            else
            { hover = true; selected = true; }

            foreach (var reg in DetectionRegions)
            {
                reg.Hover = hover;
                reg.Selected = selected;
            }
        }
    }
}
