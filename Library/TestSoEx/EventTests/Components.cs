namespace SoEx.TestSoEx.EventTests
{
    public interface ITheEvent
    {
        Task OnTheEvent(TheEvent theEvent);
    }

    public class TheEvent
    {
        public required string Message { get; set; }
    }

    public interface IAnotherEvent
    {
        Task OnAnotherEvent(AnotherEvent anotherEvent);
    }

    public class AnotherEvent
    {
        public required string Message { get; set; }
    }

    public interface IComboEvent
    {
        Task OnTheEvent(TheEvent theEvent);
        Task OnAnotherEvent(AnotherEvent anotherEvent);
    }

    public interface ISeparateEvent
    {
        Task OnSeparateEvent(SeparateEvent separateEvent);
    }

    public class SeparateEvent
    {
        public required string Message { get; set; }
    }

    public interface ISenderManager
    {
        Task SendMessage(string message);
    }

    public class SenderManager : ISenderManager
    {
        public async Task SendMessage(string message)
        {
            ITheEvent theEvent = Proxy.ForService<ITheEvent>();
            await theEvent.OnTheEvent(new TheEvent() { Message = message });
        }
    }

    public interface IAnotherSenderManager
    {
        Task SendMessage(string message);
    }

    public class AnotherSenderManager : IAnotherSenderManager
    {
        public async Task SendMessage(string message)
        {
            IAnotherEvent anotherEvent = Proxy.ForService<IAnotherEvent>();
            await anotherEvent.OnAnotherEvent(new AnotherEvent() { Message = message });
        }
    }

    public interface IComboSenderManager
    {
        Task SendTheMessage(string message);
        Task SendAnotherMessage(string message);
        Task SendSeparateMessage(string message);
    }

    public class ComboSenderManager : IComboSenderManager
    {
        public async Task SendTheMessage(string message)
        {
            IComboEvent comboEvent = Proxy.ForService<IComboEvent>();
            await comboEvent.OnTheEvent(new TheEvent() { Message = message });
        }
        public async Task SendAnotherMessage(string message)
        {
            IComboEvent comboEvent = Proxy.ForService<IComboEvent>();
            await comboEvent.OnAnotherEvent(new AnotherEvent() { Message = message });
        }
        public async Task SendSeparateMessage(string message)
        {
            ISeparateEvent separateEvent = Proxy.ForService<ISeparateEvent>();
            await separateEvent.OnSeparateEvent(new SeparateEvent() { Message = message });
        }
    }

    public interface IComboReceiverManager : IComboEvent, ISeparateEvent { }
    public class ComboReceiverManager : IComboReceiverManager
    {
        protected readonly IEventMonitor _eventMonitor;
        public ComboReceiverManager(IEventMonitor eventMonitor)
        {
            _eventMonitor = eventMonitor;
        }
        public async Task OnTheEvent(TheEvent theEvent)
        {
            _eventMonitor.RecordEvent($"{nameof(OnTheEvent)}-{theEvent.Message}");
            await Task.CompletedTask;
        }
        public async Task OnAnotherEvent(AnotherEvent anotherEvent)
        {
            _eventMonitor.RecordEvent($"{nameof(OnAnotherEvent)}-{anotherEvent.Message}");
            await Task.CompletedTask;
        }
        public async Task OnSeparateEvent(SeparateEvent separateEvent)
        {
            _eventMonitor.RecordEvent($"{nameof(OnSeparateEvent)}-{separateEvent.Message}");
            await Task.CompletedTask;
        }
    }

    public interface IReceiverManager : ITheEvent { }
    public class ReceiverManager : IReceiverManager
    {
        protected readonly IEventMonitor _eventMonitor;
        public ReceiverManager(IEventMonitor eventMonitor)
        {
            _eventMonitor = eventMonitor;
        }
        public virtual async Task OnTheEvent(TheEvent theEvent)
        {
            _eventMonitor.RecordEvent(theEvent.Message);
            await Task.CompletedTask;
        }
    }

    public interface IReceiverManager2 : ITheEvent { }
    public class ReceiverManager2 : ReceiverManager, IReceiverManager2
    {
        public ReceiverManager2(IEventMonitor eventMonitor) : base(eventMonitor) { }
        public override async Task OnTheEvent(TheEvent theEvent)
        {
            _eventMonitor.RecordEvent($"{theEvent.Message}-2");
            await Task.CompletedTask;
        }
    }

    public interface ISenderParentManager
    {
        Task SendTheMessage(string message);
        Task SendAnotherMessage(string message);
    }
    public class SenderParentManager : ISenderParentManager
    {
        public async Task SendTheMessage(string message)
        {
            await Proxy.ForService<ISenderManager>().SendMessage(message);
        }
        public async Task SendAnotherMessage(string message)
        {
            await Proxy.ForService<IAnotherSenderManager>().SendMessage(message);
        }
    }

    public interface IMultiEventSenderManager
    {
        Task SendTheMessage(string message);
        Task SendAnotherMessage(string message);
    }

    public class MultiEventSenderManager : IMultiEventSenderManager
    {
        public async Task SendTheMessage(string message)
        {
            ITheEvent theEvent = Proxy.ForService<ITheEvent>();
            await theEvent.OnTheEvent(new TheEvent() { Message = message });
        }
        public async Task SendAnotherMessage(string message)
        {
            IAnotherEvent anotherEvent = Proxy.ForService<IAnotherEvent>();
            await anotherEvent.OnAnotherEvent(new AnotherEvent() { Message = message });
        }
    }

    public interface IMultiEventReceiverManager : ITheEvent, IAnotherEvent { }
    public class MultiEventReceiverManager : IMultiEventReceiverManager
    {
        protected readonly IEventMonitor _eventMonitor;
        public MultiEventReceiverManager(IEventMonitor eventMonitor)
        {
            _eventMonitor = eventMonitor;
        }
        public virtual async Task OnTheEvent(TheEvent theEvent)
        {
            _eventMonitor.RecordEvent(theEvent.Message);
            await Task.CompletedTask;
        }
        public virtual async Task OnAnotherEvent(AnotherEvent anotherEvent)
        {
            _eventMonitor.RecordEvent(anotherEvent.Message);
            await Task.CompletedTask;
        }
    }

    public interface ISenderReceiverManager : ISenderManager, IReceiverManager { }
    public class SenderReceiverManager : ISenderReceiverManager
    {
        private readonly IEventMonitor _eventMonitor;
        public SenderReceiverManager(IEventMonitor eventMonitor)
        {
            _eventMonitor = eventMonitor;
        }
        public async Task SendMessage(string message)
        {
            ITheEvent theEvent = Proxy.ForService<ITheEvent>();
            await theEvent.OnTheEvent(new TheEvent() { Message = message });
        }
        public async Task OnTheEvent(TheEvent theEvent)
        {
            _eventMonitor.RecordEvent(theEvent.Message);
            await Task.CompletedTask;
        }
    }
}
