using System.Collections.Generic;


    public class InteractionHandlerFactory
    {
        private readonly Dictionary<System.Type, IInteractionHandler> _interactionHandlers = new Dictionary<System.Type, IInteractionHandler>();

        public InteractionHandlerFactory(params IInteractionHandler[] handlers)
        {
            _interactionHandlers = new Dictionary<System.Type, IInteractionHandler>();

            foreach (var handler in handlers)
            {
                _interactionHandlers[handler.InteractableType] = handler;
            }
        }

        public bool TryHandleInteraction(IInteractable interactable)
        {
            if (_interactionHandlers.TryGetValue(interactable.GetType(), out var handler))
            {
                return handler.HandleInteraction(interactable);
            }

            return false;
        }

        public void Dispose()
        {
            _interactionHandlers.Clear();
        }
    }
