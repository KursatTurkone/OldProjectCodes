using System.Collections;
using System.Collections.Generic;
using FreshAir.Systems.FreshAirInteractions;
using UnityEngine;
using FreshAir.Games.FirstRunner;

public class SpesificGameInteractor : MonoBehaviour, IInteractor
{
    [SerializeField] private PlayerNumberController _playerNumberController;
    [SerializeField] private Movement _movement;

    private InteractionHandlerFactory m_interactionHandlerFactory;

    private void Awake()
    {
        InitializeFactoryHandlers();
    }

    private void InitializeFactoryHandlers()
    {
        m_interactionHandlerFactory = new InteractionHandlerFactory(

            new CollectInteractionHandler(this, _playerNumberController),
            new ObstacleInteractionHandler(this, _playerNumberController, _movement),
            new JumperInteractionHandler(this, _movement),
            new FinishLineInteractionHandler(this, _movement),
            new EndingBlocksInteractionHandler(this, _movement, _playerNumberController),
            new FallPlaneInteractionHandler(this, _playerNumberController),
            new BridgeInteractionHandler(this)
        );
    }

    public bool TryToInteract(IInteractable interectable)
    {
        return m_interactionHandlerFactory.TryHandleInteraction(interectable);
    }

    private void OnDestroy()
    {
        _playerNumberController = null;
        _movement = null;
        m_interactionHandlerFactory.Dispose();
        m_interactionHandlerFactory = null;
    }
}