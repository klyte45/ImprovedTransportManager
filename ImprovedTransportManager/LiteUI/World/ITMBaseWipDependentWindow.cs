//using ColossalFramework.UI;
//using Kwytto.LiteUI;
//using Kwytto.Utils;
//using System.Linq;
//using UnityEngine;

//namespace ImprovedTransportManager.UI
//{
//    public abstract class ITMBaseWipDependentWindow<T, WIP> : GUIOpacityChanging where WIP : WorldInfoPanel where T : ITMBaseWipDependentWindow<T, WIP>
//    {
//        protected override bool showOverModals => false;
//        protected override bool requireModal => false;
//        protected override bool ShowCloseButton => false;
//        protected override bool ShowMinimizeButton => true;
//        protected override float FontSizeMultiplier => .9f;

//        protected abstract bool Resizable { get; }
//        protected abstract string InitTitle { get; }
//        protected abstract Vector2 StartSize { get; }
//        protected abstract Vector2 StartPosition { get; }
//        protected virtual Vector2 MinSize { get; } = default;
//        protected virtual Vector2 MaxSize { get; } = default;
//        protected abstract Tuple<UIComponent, WIP>[] ComponentsWatching { get; }

//        public static T Instance
//        {
//            get
//            {
//                if (instance == null)
//                {
//                    instance = GameObjectUtils.CreateElement<T>(UIView.GetAView().transform);
//                    instance.Visible = false;
//                }
//                return instance;
//            }
//        }

//        private void Init() => Init(InitTitle, new Rect(StartPosition, StartSize), Resizable, true, MinSize, MaxSize);

//        private static T instance;
//        protected InstanceID m_currentId;

//        public sealed override void Awake()
//        {
//            base.Awake();
//            Init();
//            OnAwake();
//        }
//        public virtual void OnAwake() { }

//        protected void FixedUpdate()
//        {
//            if (ComponentsWatching.FirstOrDefault(x => x.First.isVisible) is Tuple<UIComponent, WIP> window)
//            {
//                if (m_currentId != WorldInfoPanel.GetCurrentInstanceID())
//                {
//                    m_currentId = WorldInfoPanel.GetCurrentInstanceID();
//                    if (m_currentId.RawData > 0)
//                    {
//                        Visible = true;
//                    }
//                    OnIdChanged(m_currentId);
//                }
//                OnFixedUpdateIfVisible();
//            }
//            else
//            {
//                Visible = false;
//                m_currentId = default;
//            }
//        }

//        protected virtual void OnFixedUpdateIfVisible() { }

//        protected abstract void OnIdChanged(InstanceID currentId);

//        protected override void OnWindowDestroyed() => instance = null;
//    }
//}
