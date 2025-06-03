using FreshAir.Systems.FreshAirInteractions;
using Cinemachine.Utility;
using System;
using UnityEngine;

namespace FreshAir.Games.FirstRunner
{
    public class CollectInteractionHandler : IInteractionHandler
    {
        private PlayerNumberController _playerNumberController;

        public CollectInteractionHandler(IInteractor interactor, PlayerNumberController playerNumberController) : base(interactor)
        {
            _playerNumberController = playerNumberController;
        }

        public override Type InteractableType => typeof(Collect);

        public override bool HandleInteraction(IInteractable interactable)
        {
            interactable.GetTransform().TryGetComponent(out Collect collectBall);

            if (_playerNumberController.CurrentNumber >= _playerNumberController.Datas.Length - 1)
                return false;

            if (collect.collectableTypes.typeName ==
                _playerNumberController.Datas[_playerNumberController.CurrentNumber].typeName)
            {
                _playerNumberController.IncreaseMyNumber();
                return true;
            }

            return false;
        }
    }

    public class ObstacleInteractionHandler : IInteractionHandler
    {
        private PlayerNumberController _playerNumberController;
        private Movement _movement;

        public ObstacleInteractionHandler(IInteractor interactor, PlayerNumberController playerNumberController, RollMovement movement) : base(interactor)
        {
            _playerNumberController = playerNumberController;
            _movement = movement;
        }

        public override Type InteractableType => typeof(Obstacle);

        public override bool HandleInteraction(IInteractable interactable)
        {
            interactable.GetTransform().TryGetComponent(out Obstacle obstacle);

            if (!obstacle.CanInteract)
                return false;

            _playerNumberController.DecreaseMyNumber();
            _movement.Jump(100f);
            _movement.Knockback(15f);

            return true;
        }
    }

    public class JumperInteractionHandler : IInteractionHandler
    {
        private Movement _movement;

        public JumperInteractionHandler(IInteractor interactor, Movement movement) : base(interactor)
        {
            _movement = movement;
        }

        public override Type InteractableType => typeof(Jumper);

        public override bool HandleInteraction(IInteractable interactable)
        {
            interactable.GetTransform().TryGetComponent(out Jumper jumper);

            if (jumper.canJump)
            {
                _movement.Jump(jumper.JumpAmount);
                return true;
            }

            return false;
        }
    }

    public class FinishLineInteractionHandler : IInteractionHandler
    {
        private Movement _movement;

        public FinishLineInteractionHandler(IInteractor interactor, Movement movement) : base(interactor)
        {
            _movement = movement;
        }

        public override Type InteractableType => typeof(FinishLine);

        public override bool HandleInteraction(IInteractable interactable)
        {
            _movement.EndGameStart();
            return true;
        }
    }

    public class EndingBlocksInteractionHandler : IInteractionHandler
    {
        private PlayerNumberController _playerNumberController;
        private Movement _movement;

        public EndingBlocksInteractionHandler(IInteractor interactor, Movement movement, PlayerNumberController playerNumberController) : base(interactor)
        {
            _movement = movement;
            _playerNumberController = playerNumberController;
        }

        public override Type InteractableType => typeof(EndingBlocks);

        public override bool HandleInteraction(IInteractable interactable)
        {
            interactable.GetTransform().TryGetComponent(out EndingBlocks endingBlocks);

            if (endingBlocks.Data.typeName ==
                _playerNumberController.Datas[_playerNumberController.CurrentNumber].typeName)
            {
                _movement.EndGameEnd(endingBlocks.transform);
            }

            return true;
        }
    }

    public class FallPlaneInteractionHandler : IInteractionHandler
    {
        private PlayerNumberController _playerNumberController;

        public FallPlaneInteractionHandler(IInteractor interactor, PlayerNumberController playerNumberController) : base(interactor)
        {
            _playerNumberController = playerNumberController;
        }

        public override Type InteractableType => typeof(FallPlane);

        public override bool HandleInteraction(IInteractable interactable)
        {
            _playerNumberController.ResetNumber();
            return true;
        }
    }

    public class BridgeInteractionHandler : IInteractionHandler
    {
        public BridgeInteractionHandler(IInteractor interactor) : base(interactor)
        {
        }

        public override Type InteractableType => typeof(BridgeDetector);

        public override bool HandleInteraction(IInteractable interactable)
        {
            return true;
        }
    }

}
