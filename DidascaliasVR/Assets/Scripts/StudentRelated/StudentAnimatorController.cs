using System.Collections.Generic;
using UnityEngine;

namespace Didascalia.Student
{
    [RequireComponent(typeof(Animator))]
    internal class StudentAnimatorController : MonoBehaviour
    {
        private Animator animator = null;
        public Animator Animator => animator;
        
        // top-level states
        public static readonly int HashIsOnFoot =   Animator.StringToHash("OnFoot");

        // other
        public static readonly int HashFloatSpeed = Animator.StringToHash("Speed");


        // Standing
        public static readonly int HashTriggerEnterDesk =           Animator.StringToHash("TriggerEnterDesk");
        public static readonly int HashTriggerExitDesk =            Animator.StringToHash("TriggerExitDesk");
        public static readonly int HashTriggerTurnLeft =            Animator.StringToHash("TriggerTurnLeft");
        public static readonly int HashTriggerTurnRight =           Animator.StringToHash("TriggerTurnRight");
        public static readonly int HashIsFloor =                    Animator.StringToHash("IsFloor");
        public static readonly int HashIsFloorAnxiety =             Animator.StringToHash("IsFloorAnxiety");
        public static readonly int HashIsFloorAnxietyTEA =          Animator.StringToHash("IsFloorAnxietyTEA");
        public static readonly int HashTriggerOpenDoorOutside =     Animator.StringToHash("TriggerOpenDoorOutside");
        public static readonly int HashTriggerOpenDoorInside =      Animator.StringToHash("TriggerOpenDoorInside");
        public static readonly int HashTriggerCloseDoorOutside =    Animator.StringToHash("TriggerCloseDoorOutside");
        public static readonly int HashTriggerCloseDoorInside =     Animator.StringToHash("TriggerCloseDoorInside");
        public static readonly int HashIsGrabClassMaterial =        Animator.StringToHash("IsGrabClassMaterial");
        // public static readonly int HashIsGrabClassMaterialIdle = Animator.StringToHash("IsGrabClassMaterialIdle");
        public static readonly int HashIsBotherStanding =           Animator.StringToHash("IsBotherStanding");
        public static readonly int HashIsBotherStandingTEA =        Animator.StringToHash("IsBotherStandingTEA");


        // Sitting
        public static readonly int HashIsPayingAttention1 =             Animator.StringToHash("IsPayingAttention1");
        public static readonly int HashIsPayingAttention2 =             Animator.StringToHash("IsPayingAttention2");
        public static readonly int HashIsHandRaised =                   Animator.StringToHash("IsHandRaised");
        public static readonly int HashIsScared =                       Animator.StringToHash("IsScared");
        public static readonly int HashIsBored =                        Animator.StringToHash("IsBored");
        public static readonly int HashIsPhoning =                      Animator.StringToHash("IsPhoning");
        public static readonly int HashIsLookingBack =                  Animator.StringToHash("IsLookingBack");
        // public static readonly int HashIsTalkingBack = Animator.StringToHash("IsTalkingBack");
        public static readonly int HashTriggerTalkCalm =                Animator.StringToHash("TriggerTalkCalm");
        public static readonly int HashTriggerTalkAnxious =             Animator.StringToHash("TriggerTalkAnxious");
        public static readonly int HashTriggerAnnoyLeft =               Animator.StringToHash("TriggerAnnoyLeft");
        public static readonly int HashTriggerAnnoyRight =              Animator.StringToHash("TriggerAnnoyRight");
        public static readonly int HashTriggerPlaceForgottenMaterial =  Animator.StringToHash("TriggerPlaceForgottenMaterial");
        public static readonly int HashTriggerGetMaterialOut =          Animator.StringToHash("TriggerGetMaterialOut");
        public static readonly int HashIsWriting =                      Animator.StringToHash("IsWriting");
        public static readonly int HashIsDrawing =                      Animator.StringToHash("IsDrawing");
        public static readonly int HashIsGetMaterialOutWrong =          Animator.StringToHash("IsGetMaterialOutWrong");
        public static readonly int HashIsLaughing =                     Animator.StringToHash("IsLaughing");
        public static readonly int HashIsLaughingAlternative =          Animator.StringToHash("IsLaughingAlternative");
        public static readonly int HashIsLaughingPointing =             Animator.StringToHash("IsLaughingPointing");
        public static readonly int HashIsIdlingLeft =                   Animator.StringToHash("IsIdlingLeft");
        public static readonly int HashIsTalkingLeft =                  Animator.StringToHash("IsTalkingLeft");
        public static readonly int HashIsLaughingLeft =                 Animator.StringToHash("IsLaughingLeft");
        public static readonly int HashIsLaughingPointingLeft =         Animator.StringToHash("IsLaughingPointingLeft");
        public static readonly int HashIsIdlingRight =                  Animator.StringToHash("IsIdlingRight");
        public static readonly int HashIsTalkingRight =                 Animator.StringToHash("IsTalkingRight");
        public static readonly int HashIsLaughingRight =                Animator.StringToHash("IsLaughingRight");
        public static readonly int HashIsLaughingPointingRight =        Animator.StringToHash("IsLaughingPointingRight");
        public static readonly int HashIsAnxious =                      Animator.StringToHash("IsAnxious");
        public static readonly int HashIsAnxiousAlternative1 =          Animator.StringToHash("IsAnxiousAlternative1");
        public static readonly int HashIsAnxiousAlternative2 =          Animator.StringToHash("IsAnxiousAlternative2");
        public static readonly int HashIsCrying =                       Animator.StringToHash("IsCrying");
        public static readonly int HashIsCalmingDown =                  Animator.StringToHash("IsCalmingDown");
        public static readonly int HashIsJustifying =                   Animator.StringToHash("IsJustifying");
        public static readonly int HashIsAnnoyed =                      Animator.StringToHash("IsAnnoyed");
        public static readonly int HashIsAnnoyedLeft =                  Animator.StringToHash("IsAnnoyedLeft");
        public static readonly int HashIsAnnoyedRight =                 Animator.StringToHash("IsAnnoyedRight");
        public static readonly int HashIsTalkingFront =                 Animator.StringToHash("IsTalkingFront");
        public static readonly int HashIsBotheringLeft =                Animator.StringToHash("IsBotheringLeft");
        public static readonly int HashIsBotheringRight =               Animator.StringToHash("IsBotheringRight");
        public static readonly int HashIsIdlingTEA =                    Animator.StringToHash("IsIdlingTEA");
        public static readonly int HashIsLostSightTEA =                 Animator.StringToHash("IsLostSightTEA");
        public static readonly int HashIsTalkingCalmlyTEA =             Animator.StringToHash("IsTalkingCalmlyTEA");
        public static readonly int HashIsTalkingAnxiouslyTEA =          Animator.StringToHash("IsTalkingAnxiouslyTEA");
        public static readonly int HashIsStimulatedTEA =                Animator.StringToHash("IsStimulatedTEA");


        public enum TriggerParameter
        {
            None,
            EnterDesk,
            ExitDesk,
            TurnLeft,
            TurnRight,
            OpenDoorOutside,
            OpenDoorInside,
            CloseDoorOutside,
            CloseDoorInside,
            TalkCalm,
            TalkAnxious,
            AnnoyLeft,
            AnnoyRight,
            PlaceForgottenMaterial,
            GetMaterialOut
        }
        public enum BooleanParameter
        {
            None,
            OnFoot,
            IsFloor,
            IsFloorAnxiety,
            IsFloorAnxietyTEA,
            IsGrabClassMaterial,
            // IsGrabClassMaterialIdle,
            IsBotherStanding,
            IsBotherStandingTEA,
            IsPayingAttention1,
            IsPayingAttention2,
            IsHandRaised,
            IsScared,
            IsBored,
            IsPhoning,
            IsLookingBack,
            // IsTalkingBack,
            IsWriting,
            IsDrawing,
            IsGetMaterialOutWrong,
            IsLaughing,
            IsLaughingAlternative,
            IsLaughingPointing,
            IsIdlingLeft,
            IsTalkingLeft,
            IsLaughingLeft,
            IsLaughingPointingLeft,
            IsIdlingRight,
            IsTalkingRight,
            IsLaughingRight,
            IsLaughingPointingRight,
            IsAnxious,
            IsAnxiousAlternative1,
            IsAnxiousAlternative2,
            IsCrying,
            IsCalmingDown,
            IsJustifying,
            IsAnnoyed,
            IsAnnoyedLeft,
            IsAnnoyedRight,
            IsTalkingFront,
            IsBotheringLeft,
            IsBotheringRight,
            IsIdlingTEA,
            IsLostSightTEA,
            IsTalkingCalmlyTEA,
            IsTalkingAnxiouslyTEA,
            IsStimulatedTEA
        }

        private static readonly HashSet<int> ValidTriggerParameterHashes = new HashSet<int>
        {
            HashTriggerEnterDesk,
            HashTriggerExitDesk,
            HashTriggerTurnLeft,
            HashTriggerTurnRight,
            HashTriggerOpenDoorOutside,
            HashTriggerOpenDoorInside,
            HashTriggerCloseDoorOutside,
            HashTriggerCloseDoorInside,
            HashTriggerTalkCalm,
            HashTriggerTalkAnxious,
            HashTriggerAnnoyLeft,
            HashTriggerAnnoyRight,
            HashTriggerPlaceForgottenMaterial,
            HashTriggerGetMaterialOut
        };
        private static readonly HashSet<int> ValidBooleanParameterHashes = new HashSet<int>
        {
            HashIsOnFoot,


            HashIsFloor,
            HashIsFloorAnxiety,
            HashIsFloorAnxietyTEA,
            HashIsGrabClassMaterial,
            // HashIsGrabClassMaterialIdle,
            HashIsBotherStanding,
            HashIsBotherStandingTEA,
            HashIsPayingAttention1,
            HashIsPayingAttention2,
            HashIsHandRaised,
            HashIsScared,
            HashIsBored,
            HashIsPhoning,
            HashIsLookingBack,
            // HashIsTalkingBack,
            HashIsWriting,
            HashIsDrawing,
            HashIsGetMaterialOutWrong,
            HashIsLaughing,
            HashIsLaughingAlternative,
            HashIsLaughingPointing,
            HashIsIdlingLeft,
            HashIsTalkingLeft,
            HashIsLaughingLeft,
            HashIsLaughingPointingLeft,
            HashIsIdlingRight,
            HashIsTalkingRight,
            HashIsLaughingRight,
            HashIsLaughingPointingRight,
            HashIsAnxious,
            HashIsAnxiousAlternative1,
            HashIsAnxiousAlternative2,
            HashIsCrying,
            HashIsCalmingDown,
            HashIsJustifying,
            HashIsAnnoyed,
            HashIsAnnoyedLeft,
            HashIsAnnoyedRight,
            HashIsTalkingFront,
            HashIsBotheringLeft,
            HashIsBotheringRight,
            HashIsIdlingTEA,
            HashIsLostSightTEA,
            HashIsTalkingCalmlyTEA,
            HashIsTalkingAnxiouslyTEA,
            HashIsStimulatedTEA
        };
        public uint TriggerParameterCount => (uint)ValidTriggerParameterHashes.Count;
        public uint BooleanParameterCount => (uint)ValidBooleanParameterHashes.Count;
        public uint ParameterCount => TriggerParameterCount + BooleanParameterCount;

        void Awake()
        {
            animator = GetComponent<Animator>();
            Utils.Error.DebugbreakFailUnless(animator != null, "Animator component is missing", this);
        }

        public static void EnsureBooleanHash(int hash, Object context)
        {
            Utils.Error.DebugbreakFailUnless(
                ValidBooleanParameterHashes.Contains(hash),
                $"Hash {hash} is not a valid boolean parameter hash for StudentAnimatorController",
                context
            );
        }
        public static void EnsureTriggerHash(int hash, Object context)
        {
            Utils.Error.DebugbreakFailUnless(
                ValidTriggerParameterHashes.Contains(hash),
                $"Hash {hash} is not a valid trigger parameter hash for StudentAnimatorController",
                context
            );
        }
        public void EnsureBooleanHash(int hash) => EnsureBooleanHash(hash, this);
        public void EnsureTriggerHash(int hash) => EnsureTriggerHash(hash, this);
        public void EnsureHash(int hash)
        {
            Utils.Error.DebugbreakFailUnless(
                ValidBooleanParameterHashes.Contains(hash) || ValidTriggerParameterHashes.Contains(hash),
                $"Hash {hash} is not a valid parameter hash for StudentAnimatorController",
                this
            );
        }

        public void SetBooleanParameterValue(int hash, bool value)
        {
            EnsureBooleanHash(hash);
            animator.SetBool(hash, value);
        }
        public void SetBooleanParameter(int hash) => SetBooleanParameterValue(hash, true);
        public void ResetBooleanParameter(int hash) => SetBooleanParameterValue(hash, false);

        public void SetTriggerParameter(int hash)
        {
            EnsureTriggerHash(hash);
            animator.SetTrigger(hash);
        }
        public void ResetTriggerParameter(int hash)
        {
            EnsureTriggerHash(hash);
            animator.ResetTrigger(hash);
        }


        public void SetOnFoot() => SetBooleanParameter(HashIsOnFoot);
        public void UnsetOnFoot() => ResetBooleanParameter(HashIsOnFoot);


        public static int HashFromTriggerParameter(TriggerParameter parameter)
        {
            int HashInvalidParameter()
            {
                Didascalia.Utils.Error.DebugbreakFailMessage($"Invalid TriggerParameter: {parameter}", null);
                return -1;
            }
            var result = parameter switch
            {
                TriggerParameter.None =>                    HashInvalidParameter(),
                TriggerParameter.EnterDesk =>               HashTriggerEnterDesk,
                TriggerParameter.ExitDesk =>                HashTriggerExitDesk,
                TriggerParameter.TurnLeft =>                HashTriggerTurnLeft,
                TriggerParameter.TurnRight =>               HashTriggerTurnRight,
                TriggerParameter.OpenDoorOutside =>         HashTriggerOpenDoorOutside,
                TriggerParameter.OpenDoorInside =>          HashTriggerOpenDoorInside,
                TriggerParameter.CloseDoorOutside =>        HashTriggerCloseDoorOutside,
                TriggerParameter.CloseDoorInside =>         HashTriggerCloseDoorInside,
                TriggerParameter.TalkCalm =>                HashTriggerTalkCalm,
                TriggerParameter.TalkAnxious =>             HashTriggerTalkAnxious,
                TriggerParameter.AnnoyLeft =>               HashTriggerAnnoyLeft,
                TriggerParameter.AnnoyRight =>              HashTriggerAnnoyRight,
                TriggerParameter.PlaceForgottenMaterial =>  HashTriggerPlaceForgottenMaterial,
                TriggerParameter.GetMaterialOut =>          HashTriggerGetMaterialOut,
                _ => HashInvalidParameter(),
            };
            EnsureTriggerHash(result, null);
            return result;
        }
        public static int HashFromBooleanParameter(BooleanParameter parameter)
        {
            int HashInvalidParameter()
            {
                Didascalia.Utils.Error.DebugbreakFailMessage($"Invalid BooleanParameter: {parameter}", null);
                return -1;
            }
            var result = parameter switch
            {
                BooleanParameter.None =>                   HashInvalidParameter(),
                BooleanParameter.OnFoot =>                 HashIsOnFoot,
                BooleanParameter.IsFloor =>                HashIsFloor,
                BooleanParameter.IsFloorAnxiety =>         HashIsFloorAnxiety,
                BooleanParameter.IsFloorAnxietyTEA =>      HashIsFloorAnxietyTEA,
                BooleanParameter.IsGrabClassMaterial =>    HashIsGrabClassMaterial,
                // BooleanParameter.IsGrabClassMaterialIdle => HashIsGrabClassMaterialIdle,
                BooleanParameter.IsBotherStanding =>       HashIsBotherStanding,
                BooleanParameter.IsBotherStandingTEA =>    HashIsBotherStandingTEA,
                BooleanParameter.IsPayingAttention1 =>     HashIsPayingAttention1,
                BooleanParameter.IsPayingAttention2 =>     HashIsPayingAttention2,
                BooleanParameter.IsHandRaised =>           HashIsHandRaised,
                BooleanParameter.IsScared =>               HashIsScared,
                BooleanParameter.IsBored =>                HashIsBored,
                BooleanParameter.IsPhoning =>              HashIsPhoning,
                BooleanParameter.IsLookingBack =>          HashIsLookingBack,
                // BooleanParameter.IsTalkingBack =>          HashIsTalkingBack,
                BooleanParameter.IsWriting =>              HashIsWriting,
                BooleanParameter.IsDrawing =>              HashIsDrawing,
                BooleanParameter.IsGetMaterialOutWrong =>  HashIsGetMaterialOutWrong,
                BooleanParameter.IsLaughing =>             HashIsLaughing,
                BooleanParameter.IsLaughingAlternative =>  HashIsLaughingAlternative,
                BooleanParameter.IsLaughingPointing =>     HashIsLaughingPointing,
                BooleanParameter.IsIdlingLeft =>           HashIsIdlingLeft,
                BooleanParameter.IsTalkingLeft =>          HashIsTalkingLeft,
                BooleanParameter.IsLaughingLeft =>         HashIsLaughingLeft,
                BooleanParameter.IsLaughingPointingLeft => HashIsLaughingPointingLeft,
                BooleanParameter.IsIdlingRight =>          HashIsIdlingRight,
                BooleanParameter.IsTalkingRight =>         HashIsTalkingRight,
                BooleanParameter.IsLaughingRight =>        HashIsLaughingRight,
                BooleanParameter.IsLaughingPointingRight=> HashIsLaughingPointingRight,
                BooleanParameter.IsAnxious =>              HashIsAnxious,
                BooleanParameter.IsAnxiousAlternative1 =>  HashIsAnxiousAlternative1,
                BooleanParameter.IsAnxiousAlternative2 =>  HashIsAnxiousAlternative2,
                BooleanParameter.IsCrying =>               HashIsCrying,
                BooleanParameter.IsCalmingDown =>          HashIsCalmingDown,
                BooleanParameter.IsJustifying =>           HashIsJustifying,
                BooleanParameter.IsAnnoyed =>              HashIsAnnoyed,
                BooleanParameter.IsAnnoyedLeft =>          HashIsAnnoyedLeft,
                BooleanParameter.IsAnnoyedRight =>         HashIsAnnoyedRight,
                BooleanParameter.IsTalkingFront =>         HashIsTalkingFront,
                BooleanParameter.IsBotheringLeft =>        HashIsBotheringLeft,
                BooleanParameter.IsBotheringRight =>       HashIsBotheringRight,
                BooleanParameter.IsIdlingTEA =>            HashIsIdlingTEA,
                BooleanParameter.IsLostSightTEA =>         HashIsLostSightTEA,
                BooleanParameter.IsTalkingCalmlyTEA =>     HashIsTalkingCalmlyTEA,
                BooleanParameter.IsTalkingAnxiouslyTEA =>  HashIsTalkingAnxiouslyTEA,
                BooleanParameter.IsStimulatedTEA =>        HashIsStimulatedTEA,
                _ => HashInvalidParameter(),
            };
            EnsureBooleanHash(result, null);
            return result;
        }
    }
}