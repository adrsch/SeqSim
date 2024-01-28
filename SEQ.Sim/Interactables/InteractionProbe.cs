using Stride.Engine;
using Stride.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using Stride.Core.Mathematics;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script;
using SEQ.Sim;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public class InteractionProbe : SyncScript
    {
       //  ColliderShape ProbeShape;
      //  public float SphereRadius = 1f;
        public float RaycastDistance = 7f;
        public float DefaultFraction = 0.7f;
        public float ItemFraction = 0.7f;
        public float SmallFraction = 0.6f;
        public float BigFraction = 1f;

        Action overrideActivate;
        public bool isActivateOverrideActive;
        public void OverrieActivate(Action onActivate)
        {
            isActivateOverrideActive = true;
            overrideActivate = onActivate;
            Unfocus();
        }
        public static InteractionProbe S;
        public override void Start()
        {
            S = this;
            base.Start();
         //   ProbeShape = new SphereColliderShape(false, SphereRadius);
        }
        public override void Update()
        {
            if (GameStateManager.Inst.Active.GetInteractionState() == InteractionState.World)
            {
                if (!isActivateOverrideActive)
                {
                    DoProbe();
                }
                if (Keybinds.GetKeyDown(Keybind.Interact))
                {
                    if (isActivateOverrideActive)
                    {
                        isActivateOverrideActive = false;
                        overrideActivate?.Invoke();
                    }
                    else
                    {
                        TryActivate();
                    }
                }
                if (Keybinds.GetKeyUp(Keybind.Interact))
                {
                    TryDeactivate();
                }
            }
            else
            {
                TryDeactivate();
                Unfocus();
            }
        }

        float FlaggedTime;

        void DoProbe()
        {
            var raycastStart = Entity.Transform.WorldMatrix.TranslationVector;
            var forward = Entity.Transform.WorldMatrix.Forward;
            var raycastEnd = raycastStart + forward * (G.S.DebugMode ? 999f : RaycastDistance);

            IInteractable interactable = null;
            //var hit = this.GetSimulation().Raycast(raycastStart, raycastEnd);
            // avoid player
            var hit = this.GetSimulation().Raycast(raycastStart, raycastEnd, CollisionFilterGroups.CustomFilter2);//, CollisionFilterGroupFlags.AllFilter & ~CollisionFilterGroupFlags.CharacterFilter );

            if (G.S.DebugMode && hit.Succeeded)
            {
                DebugText.Print($"{MathUtil.RoundToInt(Vector3.Distance(hit.Point, raycastStart))}", new Int2(1024, 128));
            }
            // debug mode lets you itneract far
            if (hit.Succeeded)
            {
                interactable = hit.Collider.Entity.GetInterfaceInParent<IInteractable>();
                if (interactable != null)
                {
                    switch (interactable.DistanceClass)
                    {
                        case InteractableDistance.Default:
                            if (hit.HitFraction > DefaultFraction)
                                interactable = null;
                            break;
                        case InteractableDistance.Item:
                            if (hit.HitFraction > ItemFraction)
                                interactable = null;
                            break;
                        case InteractableDistance.Small:
                            if (hit.HitFraction > SmallFraction)
                                interactable = null;
                            break;
                        case InteractableDistance.Big:
                            if (hit.HitFraction > BigFraction)
                                interactable = null;
                            break;
                        case InteractableDistance.Disabled:
                            interactable = null;
                            break;
                    }
                }
                if (PlayerAnimator.S.IsArmed)
                {
                    var perceptible = hit.Collider.Entity.GetInterfaceInParent<IPerceptible>();
                    if (perceptible == PlayerAnimator.S)
                        Logger.Log(Channel.Gameplay, LogPriority.Warning, "Interaction probing itself");    
                    if (perceptible != null
                       && perceptible != PlayerAnimator.S
                       && perceptible.IsFacing(raycastStart))
                    {
                         if (!perceptible.IsArmed)
                        {
                            if (FlaggedTime + 1f < Time.time)
                            {
                                EventManager.Raise(new FlaggingIncident
                                {
                                    Flagger = PlayerAnimator.S,
                                    Flagged = perceptible
                                });
                                FlaggedTime = Time.time;
                            }

                            (perceptible as FactionAI)?.OnFlagged();
                        }
                    }
                }
            }

            if (interactable != null)
            {
                if (interactable == Focus && !G.S.DebugMode)
                    return;

                if (Focus !=  null)
                {
                    Unfocus();
                }
                EventManager.Raise(new InteractEvent { Target = interactable, Type = InteractType.Focus });

                Focus = interactable;
                Focus.Focus();
            }
            else
            {
                Unfocus();
            }
        }

        IInteractable Focus;
        IInteractable Activated;
        void Unfocus()
        {
            if (Focus != null)
            {
                EventManager.Raise(new InteractEvent { Target = Focus, Type = InteractType.Unfocus });
                Focus.Unfocus(Activated == Focus);
                Focus = null;
            }
        }

        void TryActivate()
        {
            if (Focus != null)
            {
                EventManager.Raise(new InteractEvent { Target = Focus, Type = InteractType.Activate });

                Activated = Focus;
                Activated.Activate();
            }
        }

        void TryDeactivate()
        {
            if (Activated != null)
            {
                EventManager.Raise(new InteractEvent { Target = Activated, Type = InteractType.Deactivate });

                Activated.Deactivate(Activated == Focus);
                Activated = null;
            }
        }
    }
}
