﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
{
    public interface IInteractor 
    {
        bool TryToInteract(IInteractable interactable);
    }
}