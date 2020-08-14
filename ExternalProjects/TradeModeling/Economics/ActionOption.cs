using System;

namespace TradeModeling.Economics
{
    public class ActionOption<OptionInfo>
    {
        public OptionInfo info;
        private Action option;

        public ActionOption(OptionInfo info, Action onExecute)
        {
            this.info = info;
            option = onExecute;
        }

        public void Execute()
        {
            option();
        }

        public ActionOption<T1> Then<T1>(Func<OptionInfo, T1> infoGenerator, Action<T1, OptionInfo> execute)
        {
            var newInfo = infoGenerator(info);
            return new ActionOption<T1>(newInfo, () =>
            {
                Execute();
                execute(newInfo, info);
            });
        }

        public ActionOption<T1> Then<T1>(Func<OptionInfo, T1> infoGenerator, Action<T1> execute = null)
        {
            var newInfo = infoGenerator(info);
            return new ActionOption<T1>(newInfo, () =>
            {
                Execute();
                execute?.Invoke(newInfo);
            });
        }
        public ActionOption<OptionInfo> Then(Action<OptionInfo> onExecute)
        {
            return new ActionOption<OptionInfo>(info, () =>
            {
                Execute();
                onExecute?.Invoke(info);
            });
        }

        public ActionOption<T1> Then<T1>(Func<OptionInfo, ActionOption<T1>> nextActionGenerator)
        {
            var nextAction = nextActionGenerator(info);
            return new ActionOption<T1>(nextAction.info, () =>
            {
                Execute();
                nextAction.Execute();
            });
        }
    }
}
