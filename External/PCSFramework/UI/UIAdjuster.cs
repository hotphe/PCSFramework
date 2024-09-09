using Cysharp.Threading.Tasks;
using UniRx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PCS.Common;

namespace PCS.UI
{
    public class UIAdjuster : MonoBehaviour
    {
        private RectTransform _adjustPanel;

        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private Vector2 _referenceScreenSize = new Vector2(1080, 1920);

        [SerializeField] private RectTransform _topPanel;
        [SerializeField] private RectTransform _centerPanel;
        [SerializeField] private RectTransform _bottomPanel;
        [SerializeField] private RectTransform _leftPanel;
        [SerializeField] private RectTransform _rightPanel;
        [SerializeField] private RectTransform _mainPanel;

        private AspectRatioFitter _mainAspectRatioFitter;

        [SerializeField] private bool _useConstantTop;
        [Condition("_useConstantTop", true)][SerializeField] private float _topHeight;
        [Condition("_useConstantTop", true)][SerializeField] private int _topPriority;

        [SerializeField] private bool _useConstantBottom;
        [Condition("_useConstantBottom", true)][SerializeField] private float _bottomHeight;
        [Condition("_useConstantBottom", true)][SerializeField] private int _bottomPriority;

        [SerializeField] private bool _useConstantLeft;
        [Condition("_useConstantLeft", true)][SerializeField] private float _leftWidth;
        [Condition("_useConstantLeft", true)][SerializeField] private int _leftPriority;

        [SerializeField] private bool _useConstantRight;
        [Condition("_useConstantRight", true)][SerializeField] private float _rightWidth;
        [Condition("_useConstantRight", true)][SerializeField] private int _rightPriority;

        
        public Vector2 ReferenceScreenSize => _referenceScreenSize;

        public bool UseConstantTop => _useConstantTop;
        public bool UseConstantBottom => _useConstantBottom;
        public bool UseConstantLeft => _useConstantLeft;
        public bool UseConstantRight => _useConstantRight;

        public RectTransform TopPanel => _topPanel;
        public RectTransform BottomPanel => _bottomPanel;
        public RectTransform CenterPanel => _centerPanel;
        public RectTransform LeftPanel => _leftPanel;
        public RectTransform RightPanel => _rightPanel;
        public RectTransform MainPanel => _mainPanel;

        public int TopPriority => _topPriority;
        public int BottomPriority => _bottomPriority;
        public int LeftPriority => _leftPriority;
        public int RightPriority => _rightPriority;

        public float TopHeight => _topHeight;
        public float BottomHeight => _bottomHeight;
        public float LeftWidth => _leftWidth;
        public float RightWidth => _rightWidth;

        public RectTransform AdjustPanel
        {
            get
            {
                if (_adjustPanel == null)
                    _adjustPanel = GetComponent<RectTransform>();
                return _adjustPanel;
            }
        }

        public AspectRatioFitter MainAspectRatioFitter
        {
            get
            {
                if( _mainAspectRatioFitter == null)
                    _mainAspectRatioFitter = _mainPanel.GetComponent<AspectRatioFitter>();
                return _mainAspectRatioFitter;
            }
        }

        private void Start()
        {
            Apply(ScreenResolutionController.DeviceResolution);
            ScreenResolutionController.OnUpdateDeviceResolution.Subscribe(Apply).AddTo(this);
        }

        private void Update()
        {
            Apply(ScreenResolutionController.DeviceResolution);
        }

        public void Apply(Vector2Int deviceResolution)
        {
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            _canvasScaler.referenceResolution = _referenceScreenSize;

            Vector2 minAnchor = Screen.safeArea.min;
            Vector2 maxAnchor = Screen.safeArea.max;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            AdjustPanel.anchorMin = minAnchor;
            AdjustPanel.anchorMax = maxAnchor;

            Vector2 midOffsetMin = new Vector2(0, 0);
            Vector2 midOffsetMax = new Vector2(0, 0);

            float topOffset;
            float bottomOffset;
            float leftOffset;
            float rightOffset;

            if (_useConstantTop)
            {
                leftOffset = 0;
                rightOffset = 0;
                if(LeftPriority > TopPriority)
                    leftOffset = LeftWidth;
                if(RightPriority > TopPriority)
                    rightOffset = RightWidth;
                _topPanel.anchorMin = new Vector2(0, 1);
                _topPanel.anchorMax = new Vector2(1, 1);
                _topPanel.offsetMin = new Vector2(leftOffset, -_topHeight);
                _topPanel.offsetMax = new Vector2(-rightOffset, 0);
                midOffsetMax.y = -_topHeight;
            }

            if(_useConstantBottom)
            { 
                leftOffset = 0;
                rightOffset = 0;
                if (LeftPriority > BottomPriority)
                    leftOffset = LeftWidth;
                if (RightPriority > BottomPriority)
                    rightOffset = RightWidth;
                _bottomPanel.anchorMin = new Vector2(0, 0);
                _bottomPanel.anchorMax = new Vector2(1, 0);
                _bottomPanel.offsetMin = new Vector2(leftOffset, 0);
                _bottomPanel.offsetMax = new Vector2(-rightOffset,_bottomHeight);
                midOffsetMin.y = _bottomHeight;
            }

            if(_useConstantLeft)
            {
                topOffset = 0;
                bottomOffset = 0;
                if (TopPriority >= LeftPriority)
                    topOffset = TopHeight;
                if (BottomPriority >= LeftPriority)
                    bottomOffset = BottomHeight;
                _leftPanel.anchorMin = new Vector2(0, 0);
                _leftPanel.anchorMax = new Vector2(0, 1);
                _leftPanel.offsetMin = new Vector2(0, bottomOffset);
                _leftPanel.offsetMax = new Vector2(_leftWidth, -topOffset);
                midOffsetMin.x = _leftWidth;
            }

            if(_useConstantRight) 
            {
                topOffset = 0;
                bottomOffset = 0;
                if (TopPriority >= RightPriority)
                    topOffset = TopHeight;
                if (BottomPriority >= RightPriority)
                    bottomOffset = BottomHeight;
                _rightPanel.anchorMin = new Vector2(1, 0);
                _rightPanel.anchorMax = new Vector2(1, 1);
                _rightPanel.offsetMin = new Vector2(-_rightWidth, bottomOffset);
                _rightPanel.offsetMax = new Vector2(0, -topOffset);
                midOffsetMax.x = -_rightWidth;
            }

            _centerPanel.anchorMin = new Vector2(0, 0);
            _centerPanel.anchorMax = new Vector2(1, 1);
            _centerPanel.offsetMin = midOffsetMin;
            _centerPanel.offsetMax = midOffsetMax;

            MainAspectRatioFitter.aspectRatio = _referenceScreenSize.x / _referenceScreenSize.y;

            if (IsLongWidth())
            {
                MainAspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            }else
            {
                MainAspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            }

            _mainPanel.offsetMin = Vector2.zero;
            _mainPanel.offsetMax = Vector2.zero;
            _mainPanel.localPosition = Vector3.zero;
        }

        private bool IsLongWidth()
        {
            return _referenceScreenSize.x / _referenceScreenSize.y < Screen.width / (float)Screen.height;
        }
    }
}