
using System.Collections.Generic;
using System;using SEQ.Script;
using SEQ.Script.Core;
using SEQ.Sim;

namespace SEQ.Sim
{
    public static class PlayerData
    {
        static ActorState cachedState;
        public static ActorState State
        {
            get
            {
                if (cachedState == null)
                    cachedState = ActorState.Get("player");
                
                return cachedState;
            }
        }
        public static ActorState Entity => State;
        public static void Init()
        {
            EventManager.AddListener<NewSaveEvent>(x => Reset());
            Reset();
        }
        public static void Reset()
        {
            cachedState = null;
        }

        public static void TryAddEntityToInventory(Actor e)
        {
            var state = e.State;
            if (state.GetSpecies() is ActorSpecies spawner)
            {
                if (!InventoryFull())
                {
                    if (spawner.Stackable)
                    {
                        var added = false;
                        foreach (var c in State.Children)
                        {
                            var childE = ActorState.Get(c);
                            if (childE != null && childE.Species == e.State.Species)
                            {
                                childE.Quantity += e.State.Quantity;
                                State.OnChanged();
                                added = true;
                                break;
                            }

                        }
                        if (!added)
                        {
                            State.AddChild(state.SeqId);
                        }
                    }
                    else
                    {
                        State.AddChild(state.SeqId);
                    }
                    state.DestroyWorld();
                }
                else
                {
                    ScriptRunner.Exec("misc/invfull");
                }
            }
            else
            {
                Logger.Log(Channel.Data, LogPriority.Error, $"Can't add to inv item w/o spawner {e.State.SeqId} {e.State.Species}");
            }
        }

        public static bool InventoryFull()
        {
            return ActorState.Get("player").Children.Count >= 10;
        }
    }
}