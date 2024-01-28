using Stride.Core;
using Stride.Engine;
using System.Collections;
using System.Collections.Generic;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;using SEQ.Sim;

namespace SEQ.Sim
{
    public abstract class ActorUsable
    {
        public ActorState Bound;
        public Prefab Prefab;
        public ActorSpecies Species;
        public IUsableUser User;
        public Entity Child;
        public static ActorUsable Get(ActorState state, IUsableUser user)
        {
            if (state != null && !string.IsNullOrWhiteSpace(state.Species))
            {
                if (SimSpeciesRegistry.TryGetSpecies(state.Species, out var s))
                {
                    var usable = new Weapon();
                    usable.Species = s;
                    usable.Prefab = user.IsPlayer() ? s.FpPrefab : null;
                    usable.Bound = state;
                    usable.User = user;
                    usable.Create();
                    return usable;
                }
                else
                {
                    Logger.Log(Channel.Data, LogPriority.Warning, $"Trying to get actor usable for actor w/o species {state.SeqId}");
                }
            }
            return null;
        }


        public void Create()
        {
            if (Child == null)
            {
                if (Prefab != null)
                {
                    Child = Prefab.InstantiateSingle(User.ParentEnt.Scene);
                    Child.SetParentAndZero(User.ParentEnt);
                }
                else
                {
                    Child = (User as EntityComponent).Entity;
                }
                OnInit();
                OnEquip();
            }
        }

        public virtual void OnInit()
        {

        }

   /*     ~EntityUsableBase()
        {
            if (Catalog.ContainsKey(SeqId) && Catalog[SeqId] == this)
                Catalog.Remove(SeqId);
        }*/

        public abstract void OnFireDown();
        public abstract void OnFireFrame();
        public abstract void OnFireUp();
        public abstract void OnAltFireDown();
        public abstract void OnAltFireFrame();
        public abstract void OnAltFireUp();

        public abstract void OnEquip();
        public abstract void OnUnequip();

        public void Equip()
        {
        }
        public bool IsReady;
        public virtual void EquipFinished()
        {
            IsReady = true;
        }

        public void Unequip()
        {
            IsReady = false;
            OnUnequip();
        }

        public virtual void FinishedUnequip()
        {

            if (Child != null && User.IsPlayer())
                Child.Destroy();
            Child = null;
        }

        public void Drop()
        {
            /*
            var spawner = Bound.GetSpawner();
            var item = spawner.GetWorldEnt(Bound);
            item.SetState(Bound);
            Vector3 normal = Vector3.up;
            Vector3 pos = PlayerMovementManager.Inst.Controller.Position;
            if (Pointer.Inst.DoDropItemRaycast(out var hit))
            {
                pos = hit.point;
                normal = hit.normal;
            }

            item.transform.rotation = Quaternion.identity;
            if (item.DropTransform != null)
            {
                var offset = item.DropTransform.position - item.transform.position;
                item.transform.position = pos - offset;
                item.transform.rotation = item.DropTransform.transform.rotation;
            }
            else
            {
                item.transform.position = pos;
            }
            item.transform.rotation *= Quaternion.FromToRotation(Vector3.up, normal);
            Bound.GetParent().RemoveChild(Bound.Id);

            gameObject.SetActive(false);
            FMODWrapper.Play("{62a6fc2d-21cf-43cc-9581-a54af7a875ce}");
            */
        }

        public virtual bool Reload()
        {
            return true;
        }
    }
}