using System.Collections.Generic;
using UnityEngine;

namespace Didascalia.Student
{
    internal class StudentAnimatorController : MonoBehaviour
    {
        [SerializeField]
        private Animator studentAnimator = null;
        public Animator StudentAnimator => studentAnimator;

        private Animator deskAnimator = null;
        public Animator DeskAnimator => deskAnimator;
        
        public void SetDeskAnimator(Animator desk)
        {
            deskAnimator = desk;
        }

        public float GetCurrentStudentAnimationDuration()
        {
            int layerIndex = studentAnimator.GetLayerIndex("Base Layer");
            AnimatorStateInfo info = studentAnimator.GetCurrentAnimatorStateInfo(layerIndex);
            return info.length;
        }

        public float GetCurrentStudentAnimationProgress()
        {
            int layerIndex = studentAnimator.GetLayerIndex("Base Layer");
            AnimatorStateInfo info = studentAnimator.GetCurrentAnimatorStateInfo(layerIndex);
            return info.normalizedTime;
        }

        // top-level states
        /// <summary>
        /// Hash para el parámetro "OnFoot" del Animator del Desk y del Student
        /// </summary>
        public static readonly int HashIsOnFoot =   Animator.StringToHash("OnFoot");

        // other
        public static readonly int HashFloatSpeed = Animator.StringToHash("Speed");

        // Los parámetros compartidos entre Desk y Student vamos a hacer que tengan el mismo nombre,
        // así evitamos problemas

        // Standing
        public static readonly int HashTriggerEnterDesk =           Animator.StringToHash("TriggerEnterDesk");
        public static readonly int HashTriggerExitDesk =            Animator.StringToHash("TriggerExitDesk");
        public static readonly int HashTriggerTurnLeft =            Animator.StringToHash("TriggerTurnLeft");
        public static readonly int HashTriggerTurnRight =           Animator.StringToHash("TriggerTurnRight");
        public static readonly int HashIsFloor =                    Animator.StringToHash("IsFloor");
        public static readonly int HashIsFloorAnxiety =             Animator.StringToHash("IsFloorAnxiety");
        public static readonly int HashTriggerOpenDoorOutside =     Animator.StringToHash("TriggerOpenDoorOutside");
        public static readonly int HashTriggerOpenDoorInside =      Animator.StringToHash("TriggerOpenDoorInside");
        public static readonly int HashTriggerCloseDoorOutside =    Animator.StringToHash("TriggerCloseDoorOutside");
        public static readonly int HashTriggerCloseDoorInside =     Animator.StringToHash("TriggerCloseDoorInside");
        public static readonly int HashTriggerGrabClassMaterial =   Animator.StringToHash("TriggerGrabClassMaterial");
        public static readonly int HashTriggerDeskMaterialOut =     Animator.StringToHash("TriggerDeskMaterialOut");
        // public static readonly int HashIsGrabClassMaterialIdle = Animator.StringToHash("IsGrabClassMaterialIdle");
        public static readonly int HashIsBotherStanding =           Animator.StringToHash("IsBotherStanding");

        // TEA
        public static readonly int HashIsBotherStandingTEA =        Animator.StringToHash("IsBotherStandingTEA");
        public static readonly int HashIsFloorAnxietyTEA =          Animator.StringToHash("IsFloorAnxietyTEA");

        // Desk
        public static readonly int HashTriggerMaterialDesk =        Animator.StringToHash("Mat");
        public static readonly int HashTriggerMaterialFailDesk =    Animator.StringToHash("Mat_Fail");
        public static readonly int HashTriggerWriteDesk =           Animator.StringToHash("Write");
        public static readonly int HashTriggerDrawDesk =            Animator.StringToHash("Draw");
        public static readonly int HashTriggerPutClassMaterialDesk= Animator.StringToHash("PutClassMaterial");

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
        public static readonly int HashIsOff =                          Animator.StringToHash("IsOff");

        public static readonly int HashIsCarryingMaterial =             Animator.StringToHash("IsCarryingMaterial");

        public static readonly int HashIsAnnoyed =                      Animator.StringToHash("IsAnnoyed");
        public static readonly int HashIsAnnoyedLeft =                  Animator.StringToHash("IsAnnoyedLeft");
        public static readonly int HashIsAnnoyedRight =                 Animator.StringToHash("IsAnnoyedRight");
        public static readonly int HashIsTalkingFront =                 Animator.StringToHash("IsTalkingFront");
        public static readonly int HashIsBotheringLeft =                Animator.StringToHash("IsBotheringLeft");
        public static readonly int HashIsBotheringRight =               Animator.StringToHash("IsBotheringRight");
         
        // SET ON FOOT -> Stand Up from different places
        public static readonly int HashTriggerStandUpFromChair =        Animator.StringToHash("StandUpFromChair");
        public static readonly int HashTriggerStandUpFromFloor =        Animator.StringToHash("StandUpFromFloor");

        // UN SET ON FOOT
        public static readonly int HashTriggerSitOnChair = Animator.StringToHash("SitOnChair"); 
        public static readonly int HashTriggerSitOnFloor = Animator.StringToHash("SitOnFloor");

        // TEA
        public static readonly int HashIsTEA =                          Animator.StringToHash("IsTEA");
        public static readonly int HashIsIdlingTEA =                    Animator.StringToHash("IsIdlingTEA");
        public static readonly int HashIsLostSightTEA =                 Animator.StringToHash("IsLostSightTEA");
        public static readonly int HashIsTalkingCalmlyTEA =             Animator.StringToHash("IsTalkingCalmlyTEA");
        public static readonly int HashIsTalkingAnxiouslyTEA =          Animator.StringToHash("IsTalkingAnxiouslyTEA");
        public static readonly int HashIsStimulatedTEA =                Animator.StringToHash("IsStimulatedTEA");
        public static readonly int HashIsTEAAnxious =                   Animator.StringToHash("IsTEAAnxious");
        public static readonly int HashIsTEAAnxiousHigh =               Animator.StringToHash("IsTEAAnxiousHigh");
        public static readonly int HashIsTEAOff =                       Animator.StringToHash("IsTEAOff");
        public static readonly int HashIsTEADistracted =                Animator.StringToHash("IsTEADistracted");

        void Awake()
        {
            Utils.Error.DebugbreakFailUnless(studentAnimator != null, "Animator component is missing", this);
        }

        #region StudentParameters
        [System.Serializable]
        public enum TriggerStudentParameter
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
        [System.Serializable]
        public enum BooleanStudentParameter
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
            IsStimulatedTEA,
            IsTEAAnxious,
            IsTEAAnxiousHigh,
        }

        private static readonly HashSet<int> ValidStudentTriggerParameterHashes = new HashSet<int>
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
            HashTriggerGetMaterialOut,

            // stand up from places
            HashTriggerStandUpFromChair,
            HashTriggerStandUpFromFloor,

            // sit down on places
            HashTriggerSitOnChair,
            HashTriggerSitOnFloor,
            HashTriggerGrabClassMaterial,
            HashTriggerDeskMaterialOut,

        };
        private static readonly HashSet<int> ValidStudentBooleanParameterHashes = new HashSet<int>
        {
            HashIsOnFoot,


            HashIsFloor,
            HashIsFloorAnxiety,
            HashIsFloorAnxietyTEA,
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

            HashIsTEA,
            HashIsIdlingTEA,
            HashIsLostSightTEA,
            HashIsTalkingCalmlyTEA,
            HashIsTalkingAnxiouslyTEA,
            HashIsStimulatedTEA,
            HashIsTEAAnxious,
            HashIsTEAAnxiousHigh,
            HashIsTEAOff,
            HashIsTEADistracted,
            HashIsCarryingMaterial,

            HashIsOff,
        };
        public uint TriggerStudentParameterCount => (uint)ValidStudentTriggerParameterHashes.Count;
        public uint BooleanStudentParameterCount => (uint)ValidStudentBooleanParameterHashes.Count;
        public uint StudentParameterCount => TriggerStudentParameterCount + BooleanStudentParameterCount;
        #endregion

        #region DeskParameters
        [System.Serializable]
        public enum TriggerDeskParameter
        {
            None,
            Mat,
            Mat_Fail
        
        }
        [System.Serializable]
        public enum BooleanDeskParameter
        {
            None,
            OnFoot,
            Write,
            Draw
        }

        private static readonly HashSet<int> ValidDeskTriggerParameterHashes = new HashSet<int>
        {
            HashTriggerGetMaterialOut,
            HashIsGetMaterialOutWrong,
            HashTriggerDrawDesk,
            HashTriggerWriteDesk,
            HashTriggerMaterialFailDesk,
            HashTriggerMaterialDesk,
            HashTriggerPutClassMaterialDesk
        };

        private static readonly HashSet<int> ValidDeskBooleanParameterHashes = new HashSet<int>
        {
            HashIsOnFoot,
            HashIsWriting,
            HashIsDrawing
        };

        public uint TriggerDeskParameterCount => (uint)ValidDeskTriggerParameterHashes.Count;
        public uint BooleanDeskParameterCount => (uint)ValidDeskBooleanParameterHashes.Count;
        public uint DeskParameterCount => TriggerDeskParameterCount + BooleanDeskParameterCount;
        #endregion


        #region StudentMethods
        public static void EnsureStudentBooleanHash(int hash, Object context)
        {
            Utils.Error.DebugbreakFailUnless(
                ValidStudentBooleanParameterHashes.Contains(hash),
                $"Hash {hash} is not a valid boolean parameter hash for StudentAnimatorController",
                context
            );
        }
        public static void EnsureStudentTriggerHash(int hash, Object context)
        {
            Utils.Error.DebugbreakFailUnless(
                ValidStudentTriggerParameterHashes.Contains(hash),
                $"Hash {hash} is not a valid trigger parameter hash for StudentAnimatorController",
                context
            );
        }
        public void EnsureStudentBooleanHash(int hash) => EnsureStudentBooleanHash(hash, this);
        public void EnsureStudentTriggerHash(int hash) => EnsureStudentTriggerHash(hash, this);
        public void EnsureStudentHash(int hash)
        {
            Utils.Error.DebugbreakFailUnless(
                ValidStudentBooleanParameterHashes.Contains(hash) || ValidStudentTriggerParameterHashes.Contains(hash),
                $"Hash {hash} is not a valid parameter hash for StudentAnimatorController",
                this
            );
        }

        public void SetStudentBooleanParameterValue(int hash, bool value)
        {
            EnsureStudentBooleanHash(hash);
            studentAnimator.SetBool(hash, value);
        }
        public void SetStudentBooleanParameter(int hash) => SetStudentBooleanParameterValue(hash, true);
        public void ResetStudentBooleanParameter(int hash) => SetStudentBooleanParameterValue(hash, false);

        public void SetStudentTriggerParameter(int hash)
        {
            EnsureStudentTriggerHash(hash);
            studentAnimator.SetTrigger(hash);
        }

        public void ResetStudentTriggerParameter(int hash)
        {
            EnsureStudentTriggerHash(hash);
            studentAnimator.ResetTrigger(hash);
        }
        #endregion

        #region DeskMethods
        public static void EnsureDeskBooleanHash(int hash, Object context)
        {
            Utils.Error.DebugbreakFailUnless(
                ValidDeskBooleanParameterHashes.Contains(hash),
                $"Hash {hash} is not a valid boolean parameter hash for StudentAnimatorController",
                context
            );
        }
        public static void EnsureDeskTriggerHash(int hash, Object context)
        {
            Utils.Error.DebugbreakFailUnless(
                ValidDeskTriggerParameterHashes.Contains(hash),
                $"Hash {hash} is not a valid trigger parameter hash for StudentAnimatorController",
                context
            );
        }
        public void EnsureDeskBooleanHash(int hash) => EnsureDeskBooleanHash(hash, this);
        public void EnsureDeskTriggerHash(int hash) => EnsureDeskTriggerHash(hash, this);
        public void EnsureDeskHash(int hash)
        {
            Utils.Error.DebugbreakFailUnless(
                ValidDeskBooleanParameterHashes.Contains(hash) || ValidDeskTriggerParameterHashes.Contains(hash),
                $"Hash {hash} is not a valid parameter hash for StudentAnimatorController",
                this
            );
        }

        public void SetDeskBoleanParameterValue(int hash, bool value)
        {
            EnsureDeskBooleanHash(hash);
            deskAnimator.SetBool(hash, value);
        }
        public void SetDeskBooleanParameter(int hash) => SetDeskBoleanParameterValue(hash, true);
        public void ResetDeskBooleanParameter(int hash) => SetDeskBoleanParameterValue(hash, false);

        public void SetDeskTriggerParameter(int hash)
        {
            EnsureDeskTriggerHash(hash);
            deskAnimator.SetTrigger(hash);
        }

        public void ResetDeskTriggerParameter(int hash)
        {
            EnsureDeskTriggerHash(hash);
            deskAnimator.ResetTrigger(hash);
        }
        #endregion


        private void SetOnFoot()
        {
            SetStudentBooleanParameter(HashIsOnFoot);
            SetDeskBooleanParameter(HashIsOnFoot);
        }

        public void StandUpFromChair() {
            SetOnFoot();
            SetStudentTriggerParameter(HashTriggerStandUpFromChair);
        }

        public void StandUpFromFloor()
        {
            SetOnFoot();
            SetStudentTriggerParameter(HashTriggerStandUpFromFloor);
        }

        public void SitDown()
        {
            // false
            ResetStudentBooleanParameter(HashIsOnFoot);
            ResetDeskBooleanParameter(HashIsOnFoot);

            // trigger
            SetStudentTriggerParameter(HashTriggerSitOnChair);
        }

        public void ExitDesk()
        {
            SetStudentTriggerParameter(HashTriggerExitDesk);
        }

        public void EnterDesk()
        {
            SetStudentTriggerParameter(HashTriggerEnterDesk);
        }

        public void OpenDoorInside(Door door)
        {
            SetStudentTriggerParameter(HashTriggerOpenDoorInside);
            door.OpenInside();
        }

        public void CloseDoorOutside(Door door)
        {
            SetStudentTriggerParameter(HashTriggerCloseDoorOutside);
            door.CloseOutside();
        }

        public void OpenDoorOutside(Door door)
        {
            SetStudentTriggerParameter(HashTriggerOpenDoorOutside);
            door.OpenOutside();
        }

        public void CloseDoorInside(Door door)
        {
            SetStudentTriggerParameter(HashTriggerCloseDoorInside);
            door.CloseInside();
        }

        public void StartTalking()
        {
            // Didascalia.Utils.Log.Warning("TODO: StartTalking", this);
            studentAnimator.SetLayerWeight(2, 0.5f);
        }

        public void StopTalking()
        {
            studentAnimator.SetLayerWeight(2, 0.0f);
            // Didascalia.Utils.Log.Warning("TODO: StopTalking", this);
        }

        public void TDAH_GetMaterialOutWrong()
        {
            SetStudentBooleanParameter(HashIsGetMaterialOutWrong);
            SetDeskTriggerParameter(HashTriggerMaterialFailDesk);
        }

        public void TDAH_Unset_GetMaterialOutWrong()
        {
            ResetStudentBooleanParameter(HashIsGetMaterialOutWrong);
        }

        public void TDAH_GetClassMaterial()
        {
            SetStudentTriggerParameter(HashTriggerGrabClassMaterial);
            SetIsCarryingMaterial(true);
        }

        public void SetIsCarryingMaterial(bool isCarrying)
        {
            if (isCarrying) SetStudentBooleanParameter(HashIsCarryingMaterial);
            else ResetStudentBooleanParameter(HashIsCarryingMaterial);
        }

        public void GetDeskMaterialOut()
        {
            SetStudentTriggerParameter(HashTriggerDeskMaterialOut);
            SetDeskTriggerParameter(HashTriggerPutClassMaterialDesk);
        }

        public void SetAnxiety_1()
        {
            SetStudentBooleanParameter(HashIsAnxiousAlternative1);
        }

        public void UnSetAnxiety_1()
        {
            ResetStudentBooleanParameter(HashIsAnxiousAlternative1);
        }

        public void SetIsCrying()
        {
            SetStudentBooleanParameter(HashIsCrying);
        }

        public void UnSetIsCrying()
        {
            ResetStudentBooleanParameter(HashIsCrying);
        }

        public void SetIsJustifying()
        {
            SetStudentBooleanParameter(HashIsCrying);
        }

        public void UnSetIsJustifying()
        {
            ResetStudentBooleanParameter(HashIsCrying);
        }

        public void SetIsOff()
        {
            SetStudentBooleanParameter(HashIsOff);
        }

        public void UnSetIsOff()
        {
            ResetStudentBooleanParameter(HashIsOff);
        }

        public void SetIsTEA(bool isTEA)
        {
            SetStudentBooleanParameterValue(HashIsTEA, isTEA);
        }

        public void TEA_StartHyperstimulation()
        {
            int rand = Random.Range(0, 2);
            int anim = rand == 0 ? HashIsTEAAnxious : HashIsStimulatedTEA;
            SetStudentBooleanParameter(anim);
        }
        public void TEA_StopHyperstimulation()
        {
            ResetStudentBooleanParameter(HashIsTEAAnxious);
            ResetStudentBooleanParameter(HashIsStimulatedTEA);
            ResetStudentBooleanParameter(HashIsTEAAnxiousHigh);
            ResetStudentBooleanParameter(HashIsTEAOff);
        }

        public void TEA_SetAnxiety()
        {
            SetStudentBooleanParameter(HashIsTEAAnxious);
        }

        public void TEA_ResetAnxiety()
        {
            ResetStudentBooleanParameter(HashIsTEAAnxious);
        }


        public void TEA_SetHighAnxiety()
        {
            // false
            ResetStudentBooleanParameter(HashIsStimulatedTEA);
            ResetStudentBooleanParameter(HashIsTEAAnxious);

            // trigger
            SetStudentBooleanParameter(HashIsTEAAnxiousHigh);
        }

        public void TEA_Off()
        {
            SetStudentBooleanParameter(HashIsTEAOff);
        }
        
        public void TEA_GetDistracted()
        {
            SetStudentBooleanParameter(HashIsTEADistracted);
        }

        public void TEA_UnSetDistracted()
        {
            ResetStudentBooleanParameter(HashIsTEADistracted);
        }


        public void GoToFloor()
        {
            // false
            ResetStudentBooleanParameter(HashIsOnFoot);

            //true
            SetStudentBooleanParameter(HashIsFloor);
            SetStudentTriggerParameter(HashTriggerSitOnFloor);
        }

        public static int HashFromTriggerParameter(TriggerStudentParameter parameter)
        {
            int HashInvalidParameter()
            {
                Didascalia.Utils.Error.DebugbreakFailMessage($"Invalid TriggerParameter: {parameter}", null);
                return -1;
            }
            var result = parameter switch
            {
                TriggerStudentParameter.None =>                    HashInvalidParameter(),
                TriggerStudentParameter.EnterDesk =>               HashTriggerEnterDesk,
                TriggerStudentParameter.ExitDesk =>                HashTriggerExitDesk,
                TriggerStudentParameter.TurnLeft =>                HashTriggerTurnLeft,
                TriggerStudentParameter.TurnRight =>               HashTriggerTurnRight,
                TriggerStudentParameter.OpenDoorOutside =>         HashTriggerOpenDoorOutside,
                TriggerStudentParameter.OpenDoorInside =>          HashTriggerOpenDoorInside,
                TriggerStudentParameter.CloseDoorOutside =>        HashTriggerCloseDoorOutside,
                TriggerStudentParameter.CloseDoorInside =>         HashTriggerCloseDoorInside,
                TriggerStudentParameter.TalkCalm =>                HashTriggerTalkCalm,
                TriggerStudentParameter.TalkAnxious =>             HashTriggerTalkAnxious,
                TriggerStudentParameter.AnnoyLeft =>               HashTriggerAnnoyLeft,
                TriggerStudentParameter.AnnoyRight =>              HashTriggerAnnoyRight,
                TriggerStudentParameter.PlaceForgottenMaterial =>  HashTriggerPlaceForgottenMaterial,
                TriggerStudentParameter.GetMaterialOut =>          HashTriggerGetMaterialOut,
                _ => HashInvalidParameter(),
            };
            EnsureStudentTriggerHash(result, null);
            return result;
        }
        public static int HashFromBooleanParameter(BooleanStudentParameter parameter)
        {
            int HashInvalidParameter()
            {
                Didascalia.Utils.Error.DebugbreakFailMessage($"Invalid BooleanParameter: {parameter}", null);
                return -1;
            }
            var result = parameter switch
            {
                BooleanStudentParameter.None =>                   HashInvalidParameter(),
                BooleanStudentParameter.OnFoot =>                 HashIsOnFoot,
                BooleanStudentParameter.IsFloor =>                HashIsFloor,
                BooleanStudentParameter.IsFloorAnxiety =>         HashIsFloorAnxiety,
                BooleanStudentParameter.IsFloorAnxietyTEA =>      HashIsFloorAnxietyTEA,
                BooleanStudentParameter.IsGrabClassMaterial =>    HashTriggerGrabClassMaterial,
                // BooleanParameter.IsGrabClassMaterialIdle => HashIsGrabClassMaterialIdle,
                BooleanStudentParameter.IsBotherStanding =>       HashIsBotherStanding,
                BooleanStudentParameter.IsBotherStandingTEA =>    HashIsBotherStandingTEA,
                BooleanStudentParameter.IsPayingAttention1 =>     HashIsPayingAttention1,
                BooleanStudentParameter.IsPayingAttention2 =>     HashIsPayingAttention2,
                BooleanStudentParameter.IsHandRaised =>           HashIsHandRaised,
                BooleanStudentParameter.IsScared =>               HashIsScared,
                BooleanStudentParameter.IsBored =>                HashIsBored,
                BooleanStudentParameter.IsPhoning =>              HashIsPhoning,
                BooleanStudentParameter.IsLookingBack =>          HashIsLookingBack,
                // BooleanParameter.IsTalkingBack =>          HashIsTalkingBack,
                BooleanStudentParameter.IsWriting =>              HashIsWriting,
                BooleanStudentParameter.IsDrawing =>              HashIsDrawing,
                BooleanStudentParameter.IsGetMaterialOutWrong =>  HashIsGetMaterialOutWrong,
                BooleanStudentParameter.IsLaughing =>             HashIsLaughing,
                BooleanStudentParameter.IsLaughingAlternative =>  HashIsLaughingAlternative,
                BooleanStudentParameter.IsLaughingPointing =>     HashIsLaughingPointing,
                BooleanStudentParameter.IsIdlingLeft =>           HashIsIdlingLeft,
                BooleanStudentParameter.IsTalkingLeft =>          HashIsTalkingLeft,
                BooleanStudentParameter.IsLaughingLeft =>         HashIsLaughingLeft,
                BooleanStudentParameter.IsLaughingPointingLeft => HashIsLaughingPointingLeft,
                BooleanStudentParameter.IsIdlingRight =>          HashIsIdlingRight,
                BooleanStudentParameter.IsTalkingRight =>         HashIsTalkingRight,
                BooleanStudentParameter.IsLaughingRight =>        HashIsLaughingRight,
                BooleanStudentParameter.IsLaughingPointingRight=> HashIsLaughingPointingRight,
                //BooleanStudentParameter.IsAnxious =>              HashIsAnxious,
                //BooleanStudentParameter.IsAnxiousAlternative1 =>  HashIsAnxiousAlternative1,
                //BooleanStudentParameter.IsAnxiousAlternative2 =>  HashIsAnxiousAlternative2,
                BooleanStudentParameter.IsCrying =>               HashIsCrying,
                BooleanStudentParameter.IsCalmingDown =>          HashIsCalmingDown,
                BooleanStudentParameter.IsJustifying =>           HashIsJustifying,
                BooleanStudentParameter.IsAnnoyed =>              HashIsAnnoyed,
                BooleanStudentParameter.IsAnnoyedLeft =>          HashIsAnnoyedLeft,
                BooleanStudentParameter.IsAnnoyedRight =>         HashIsAnnoyedRight,
                BooleanStudentParameter.IsTalkingFront =>         HashIsTalkingFront,
                BooleanStudentParameter.IsBotheringLeft =>        HashIsBotheringLeft,
                BooleanStudentParameter.IsBotheringRight =>       HashIsBotheringRight,
                BooleanStudentParameter.IsIdlingTEA =>            HashIsIdlingTEA,
                BooleanStudentParameter.IsLostSightTEA =>         HashIsLostSightTEA,
                BooleanStudentParameter.IsTalkingCalmlyTEA =>     HashIsTalkingCalmlyTEA,
                BooleanStudentParameter.IsTalkingAnxiouslyTEA =>  HashIsTalkingAnxiouslyTEA,
                BooleanStudentParameter.IsStimulatedTEA =>        HashIsStimulatedTEA,
                BooleanStudentParameter.IsTEAAnxious =>           HashIsTEAAnxious,
                BooleanStudentParameter.IsTEAAnxiousHigh =>           HashIsTEAAnxiousHigh,

                _ => HashInvalidParameter(),
            };
            EnsureStudentBooleanHash(result, null);
            return result;
        }
    }
}