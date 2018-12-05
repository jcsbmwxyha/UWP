using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace AuraEditor.Common
{
    public enum MouseEvent
    {
        Hover = 0,
        Unhover,
        Click,
        InRegion,
        OutRegion,
    }
    public enum RegionStatus
    {
        Normal = 0,
        NormalHover,
        Selected,
        SelectedHover,
        Watching,
    }

    class MouseDetectedRegion
    {
        public MouseEventCtrl.MouseEventCallBack Callback { get; set; }
        public Rect DetectionRect { get; set; }
        public int RegionIndex { get; set; }
        public int GroupIndex { get; set; }

        public void MoveRect(double moveXOffset, double moveYOffset)
        {
            Rect r = new Rect(
                DetectionRect.X + moveXOffset,
                DetectionRect.Y + moveYOffset,
                DetectionRect.Width,
                DetectionRect.Height);

            DetectionRect = r;
        }
        public void SendMouseEvent(MouseEvent mouseEvent)
        {
            Callback?.Invoke(mouseEvent);
        }
    }

    class MouseEventCtrl
    {
        public delegate void MouseEventCallBack(MouseEvent me);
        Point _pressPoint;
        List<int> beforeIndexesInMouseRegion;
        List<int> currentIndexesInMouseRegion;

        private Rect _monitorMaxRect;
        public Rect MonitorMaxRect
        {
            get => _monitorMaxRect;
            set => _monitorMaxRect = value;
        }

        private MouseDetectedRegion[] _detectionRegions;
        public MouseDetectedRegion[] DetectionRegions
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
                        DetectionRegions[_currentHoverIndex].SendMouseEvent(MouseEvent.Unhover);
                    if (value != -1)
                        DetectionRegions[value].SendMouseEvent(MouseEvent.Hover);
                    _currentHoverIndex = value;
                }
            }
        }

        public Rect MouseRect { get; set; }

        public MouseEventCtrl()
        {
            _pressPoint = new Point(-1, -1);
            _monitorMaxRect = new Rect(new Point(0, 0), new Point(0, 0));
            beforeIndexesInMouseRegion = new List<int>();
            currentIndexesInMouseRegion = new List<int>();
            _currentHoverIndex = -1;
        }

        public void OnMousePressed(Point p)
        {
            for (int i = 0; i < DetectionRegions.Length; i++)
            {
                if (DetectionRegions[i].DetectionRect.Contains(p))
                {
                    DetectionRegions[i].SendMouseEvent(MouseEvent.Click);
                    break;
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
            MouseRect = mouseRect;

            // Detect intersection
            currentIndexesInMouseRegion.Clear();
            for (int i = 0; i < DetectionRegions.Length; i++)
            {
                Rect r = DetectionRegions[i].DetectionRect;
                r.Intersect(mouseRect);

                if (r != Rect.Empty)
                {
                    currentIndexesInMouseRegion.Add(i);
                }
            }

            var leavingRegion = beforeIndexesInMouseRegion.Except(currentIndexesInMouseRegion);
            foreach (var index in leavingRegion)
                DetectionRegions[index].SendMouseEvent(MouseEvent.OutRegion);

            var newRegion = currentIndexesInMouseRegion.Except(beforeIndexesInMouseRegion);
            foreach (var index in newRegion)
                DetectionRegions[index].SendMouseEvent(MouseEvent.InRegion);

            beforeIndexesInMouseRegion = new List<int>(currentIndexesInMouseRegion);
        }
        public void OnMouseReleased()
        {
            Rect mouseRect = MouseRect;

            currentIndexesInMouseRegion.Clear();
            beforeIndexesInMouseRegion.Clear();
            _pressPoint.X = -1;
            _pressPoint.Y = -1;
            mouseRect.Width = 0;
            mouseRect.Height = 0;

            MouseRect = mouseRect;
        }
        public void OnRightTapped()
        {
        }

        public void MoveGroupRects(int groupIndex, double moveXOffset, double moveYOffset)
        {
            foreach (var reg in DetectionRegions)
            {
                if (reg.GroupIndex == groupIndex)
                {
                    reg.MoveRect(moveXOffset, moveYOffset);
                }
            }
        }
    }
}
