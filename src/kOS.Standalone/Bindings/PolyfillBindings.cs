using kOS.Safe.Binding;
using kOS.Safe;
using kOS.Safe.Encapsulation;
using kOS.Standalone;
using kOS.Standalone.Suffixed;

namespace kOS.Binding
{
    [Binding("ksp")]
    public class PolyfillBindings : SafeBindingBase
    {
        private TerminalStruct terminal;

        public override void AddTo(SafeSharedObjects shared)
        {
            terminal = new TerminalStruct(shared);

            shared.BindingMgr.AddGetter("TERMINAL", delegate { return terminal; });
            if (shared is StandaloneSharedObjects)
            {
                shared.BindingMgr.AddGetter("TIME", delegate { return new TimePolyfill(((StandaloneSharedObjects)shared).StartTime); });
                shared.BindingMgr.AddGetter("SHIP", delegate { return ((StandaloneSharedObjects)shared).StandaloneShip; });
                shared.BindingMgr.AddGetter("CORE", delegate { return ((StandaloneSharedObjects)shared).StandaloneCore; });
            }
        }
    }
}
