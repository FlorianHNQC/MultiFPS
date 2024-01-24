using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public abstract class ProjectileBase : NetworkBehaviour
    {
        public Action OnShoot { get; internal set; }
    }
}