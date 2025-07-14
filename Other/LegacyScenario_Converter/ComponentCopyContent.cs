using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRF.BNG_Framework.Scripts.Core;
using OldSB = Scenario.Core.ScenarioBehaviour;
using NewSB = Scenario.Core.World.ScenarioBehaviour;
using OldCheckable = VRTemplate.Scripts.VRComponents.CheckableComponent;
using NewCheckable = VRF.VRBehaviours.Checking.Checkable;
using OldGrabbable = BNG.Grabbable;
using NewGrabbable = VRF.BNG_Framework.Scripts.Core.Grabbable;
using OldTriggerZone = Scenario.Core.Ecs.Utilities.TriggerZone;
using NewTriggerZone = Scenario.Utilities.TriggerZone;
using OldMultiEnabler = Scenario.Core.Utilities.MultiEnabler;
using NewMultiEnabler = Scenario.Utilities.MultiEnabler;
using OldSteeringWheel = VRF.BNG_Framework.Scripts.Core.SteeringWheel;
using NewGrabbableLever = VRF.VRBehaviours.GrabbableLever;
using OldSnapZone = BNG.SnapZone;
using NewSnapZone = VRF.BNG_Framework.Scripts.Core.SnapZone;
using OldGoMultiEnabler = Scenario.Core.Utilities.GOMultiEnabler;
using NewGoMultiEnabler = Scenario.Utilities.GOMultiEnabler;
using OldIndicationArrow = VRTemplate.Scripts.VRBehaviours.IndicationArrow;
using NewIndicationArrow = VRF.VRBehaviours.IndicationArrow;
using OldGoTransformStates = GOTransformStates;
using NewGoTransformStates = VRF.VRBehaviours.GOTransformStates;
using OldTransformData = TransformData;
using NewTransformData = VRF.VRBehaviours.TransformData;
using OldTeleportDestination = BNG.TeleportDestination;
using NewTeleportDestination = VRF.BNG_Framework.Scripts.Helpers.TeleportDestination;
using OldKeyhole = VRTemplate.VRBehaviours.Keys.Keyhole;
using NewKeyhole = VRF.VRBehaviours.Keys.Keyhole;
using OldToggleSwitch = ToggleSwitch;
using NewToggleSwitch = VRF.VRBehaviours.ToggleSwitch;
using OldTriggerEvents = VRTemplate.Scripts.VRBehaviours.TriggerEvents;
using NewColliderProvider = VRF.Utils.Colliders.ColliderProvider;
using OldWorldButton = VRTemplate.Scripts.VRBehaviours.WorldButton;
using NewWorldButton = VRF.VRBehaviours.WorldButton;
using OldRaycastable = VRTemplate.Scripts.VRBehaviours.Raycastable;
using NewRaycastable = VRF.Players.Raycasting.Raycastable;
using OldGoTransformInteractable = GOTransformInteractable;
using NewGoTransformInteractable = VRF.VRBehaviours.GOTransformInteractable;

namespace LegacyCompatibilityPack2023.Converter.Content
{
    public static class ComponentCopyContent
    {
        public static void AddNewBehaviours()
        {
            AddNew<OldSB, NewSB>(ScenarioBehaviour, ScenarioBehaviourInverse);
            AddNew<OldCheckable, NewCheckable>(CheckableComponent, CheckableComponentInverse);
            AddNew<OldGrabbable, NewGrabbable>(GrabbableComponent, GrabbableComponentInverse);
            AddNew<OldTriggerZone, NewTriggerZone>(TriggerZone, TriggerZoneInverse);
            AddNew<OldMultiEnabler, NewMultiEnabler>(MultiEnabler, MultiEnablerInverse);
            AddNew<OldSteeringWheel, NewGrabbableLever>(SteeringWheel, SteeringWheelInverse); //Ищет новый SteeringWheel, вместо старого
            AddNew<OldSnapZone, NewSnapZone>(SnapZoneConvert, SnapZoneConvertInverse);
            AddNew<OldGoMultiEnabler, NewGoMultiEnabler>(GoMultiEnabler, GoMultiEnablerInverse);
            AddNew<OldIndicationArrow, NewIndicationArrow>(IndicationArrow, IndicationArrowInverse);
            AddNew<OldGoTransformStates, NewGoTransformStates>(GoTransformStates, GoTransformStatesInverse);
            AddNew<OldTeleportDestination, NewTeleportDestination>(TeleportDestination, TeleportDestinationInverse);
            AddNew<OldKeyhole, NewKeyhole>(Keyhole, KeyholeInverse);
            AddNew<OldTriggerEvents, NewColliderProvider>(ColliderProvider, ColliderProviderInverse);
            AddNew<OldToggleSwitch, NewToggleSwitch>(ToggleSwitch, ToggleSwitchInverse);
            AddNew<OldWorldButton, NewWorldButton>(WorldButton, WorldButtonInverse);
            AddNew<OldRaycastable, NewRaycastable>(Raycastable, RaycastableInverse);
            AddNew<OldGoTransformInteractable, NewGoTransformInteractable>(GoTransformInteractable, GoTransformInteractableInverse);
        }

        public static void RemoveOldBehaviours()
        {
            Remove<OldSB, NewSB>();
            Remove<OldCheckable, NewCheckable>();
            Remove<OldGrabbable, NewGrabbable>();
            Remove<OldTriggerZone, NewTriggerZone>();
            Remove<OldMultiEnabler, NewMultiEnabler>();
            Remove<OldSteeringWheel, NewGrabbableLever>();
            Remove<OldSnapZone, NewSnapZone>();
            Remove<OldGoMultiEnabler, NewGoMultiEnabler>();
            Remove<OldIndicationArrow, NewIndicationArrow>();
            Remove<OldGoTransformStates, NewGoTransformStates>();
            Remove<OldTeleportDestination, NewTeleportDestination>();
            Remove<OldKeyhole, NewKeyhole>();
            Remove<OldTriggerEvents, NewColliderProvider>();
            Remove<OldToggleSwitch, NewToggleSwitch>();
            Remove<OldWorldButton, NewWorldButton>();
            Remove<OldRaycastable, NewRaycastable>();
            Remove<OldGoTransformInteractable, NewGoTransformInteractable>();
        }

        public static void DisableOldBehaviours()
        {
            Disable<OldCheckable>();
            Disable<OldGrabbable>();
            Disable<OldTriggerZone>();
            Disable<OldMultiEnabler>();
            Disable<OldSteeringWheel>();
            Disable<OldSnapZone>();
            Disable<OldGoMultiEnabler>();
            Disable<OldIndicationArrow>();
            Disable<OldGoTransformStates>();
            Disable<OldTeleportDestination>();
            Disable<OldKeyhole>();
            Disable<OldTriggerEvents>();
            Disable<OldToggleSwitch>();
            Disable<OldWorldButton>();
            Disable<OldRaycastable>();
            Disable<OldGoTransformInteractable>();
        }

        private static void AddNew<TOld, TNew>(Action<TNew, TOld> addOld, Action<TOld, TNew> addNew)
            where TOld : Component where TNew : Component
        {
            foreach (var oldBeh in ConverterUtilities.GetAllSceneComponents<TOld>())
                ConverterUtilities.AddNew<TOld, TNew>(oldBeh, addOld);
            foreach (var newBeh in ConverterUtilities.GetAllSceneComponents<TNew>())
                ConverterUtilities.AddNew<TNew, TOld>(newBeh, addNew);
        }

        private static void Remove<TOld, TNew>() where TOld : Component where TNew : Component
        {
            foreach (var old in ConverterUtilities.GetAllSceneComponents<TOld>())
                ConverterUtilities.RemoveOld<TOld, TNew>(old);
        }

        private static void Disable<TOld>() where TOld : MonoBehaviour
        {
            foreach (var old in ConverterUtilities.GetAllSceneComponents<TOld>())
                ConverterUtilities.Disable(old);
        }

        private static void ScenarioBehaviour(NewSB dst, OldSB src) => dst.SetID(src.ID);
        private static void ScenarioBehaviourInverse(OldSB dst, NewSB src) => dst.ID = src.GetID();

        private static void CheckableComponent(NewCheckable dst, OldCheckable src) =>
            dst.CheckingTime = src.CheckingTime;

        private static void CheckableComponentInverse(OldCheckable dst, NewCheckable src) =>
            dst.CheckingTime = src.CheckingTime;

        private static void GrabbableComponent(NewGrabbable dst, OldGrabbable src)
        {
            dst.GrabButton = (GrabButton)src.GrabButton;
            dst.Grabtype = (HoldType)src.Grabtype;
            dst.GrabMechanic = (GrabType)src.GrabMechanic;
            dst.GrabPhysics = (GrabPhysics)src.GrabPhysics;

            dst.MoveVelocityForce = src.MoveVelocityForce;
            dst.MoveAngularVelocityForce = src.MoveAngularVelocityForce;
            dst.ThrowForceMultiplier = src.ThrowForceMultiplier;
            dst.ThrowForceMultiplierAngular = src.ThrowForceMultiplierAngular;

            dst.RemoteGrabbable = src.RemoteGrabbable;
            dst.GrabMechanic = (GrabType)src.GrabMechanic;
            dst.GrabSpeed = src.GrabSpeed;
            dst.RemoteGrabDistance = src.RemoteGrabDistance;

            dst.HideHandGraphics = src.RemoteGrabbable;
            dst.ParentToHands = src.RemoteGrabbable;
            dst.ParentHandModel = src.RemoteGrabbable;
            dst.SnapHandModel = src.RemoteGrabbable;
            dst.CanBeDropped = src.RemoteGrabbable;
            dst.CanBeSnappedToSnapZone = src.RemoteGrabbable;
            dst.ForceDisableKinematicOnDrop = src.RemoteGrabbable;
            dst.InstantMovement = src.RemoteGrabbable;
            dst.MakeChildCollidersGrabbable = src.RemoteGrabbable;

            dst.BreakDistance = src.BreakDistance;
            dst.handPoseType = (HandPoseType)src.handPoseType;
            //dst.SelectedHandPose = src.SelectedHandPose;
            dst.SecondaryGrabBehavior = (OtherGrabBehavior)src.SecondaryGrabBehavior;

            dst.OtherGrabbableMustBeGrabbed = src.OtherGrabbableMustBeGrabbed?.GetComponent<NewGrabbable>();
            dst.GrabPoints = src.GrabPoints;
        }

        private static void GrabbableComponentInverse(OldGrabbable dst, NewGrabbable src)
        {
            dst.GrabButton = (BNG.GrabButton)src.GrabButton;
            dst.Grabtype = (BNG.HoldType)src.Grabtype;
            dst.GrabMechanic = (BNG.GrabType)src.GrabMechanic;
            dst.GrabPhysics = (BNG.GrabPhysics)src.GrabPhysics;

            dst.MoveVelocityForce = src.MoveVelocityForce;
            dst.MoveAngularVelocityForce = src.MoveAngularVelocityForce;
            dst.ThrowForceMultiplier = src.ThrowForceMultiplier;
            dst.ThrowForceMultiplierAngular = src.ThrowForceMultiplierAngular;

            dst.RemoteGrabbable = src.RemoteGrabbable;
            dst.GrabMechanic = (BNG.GrabType)src.GrabMechanic;
            dst.GrabSpeed = src.GrabSpeed;
            dst.RemoteGrabDistance = src.RemoteGrabDistance;

            dst.HideHandGraphics = src.RemoteGrabbable;
            dst.ParentToHands = src.RemoteGrabbable;
            dst.ParentHandModel = src.RemoteGrabbable;
            dst.SnapHandModel = src.RemoteGrabbable;
            dst.CanBeDropped = src.RemoteGrabbable;
            dst.CanBeSnappedToSnapZone = src.RemoteGrabbable;
            dst.ForceDisableKinematicOnDrop = src.RemoteGrabbable;
            dst.InstantMovement = src.RemoteGrabbable;
            dst.MakeChildCollidersGrabbable = src.RemoteGrabbable;

            dst.BreakDistance = src.BreakDistance;
            dst.handPoseType = (BNG.HandPoseType)src.handPoseType;
            //dst.SelectedHandPose = src.SelectedHandPose;
            dst.SecondaryGrabBehavior = (BNG.OtherGrabBehavior)src.SecondaryGrabBehavior;

            dst.OtherGrabbableMustBeGrabbed = src.OtherGrabbableMustBeGrabbed?.GetComponent<OldGrabbable>();
            dst.GrabPoints = src.GrabPoints;
        }

        private static void TriggerZone(NewTriggerZone dst, OldTriggerZone src) => dst.enabled = src.enabled;
        private static void TriggerZoneInverse(OldTriggerZone dst, NewTriggerZone src) => dst.enabled = src.enabled;

        public static readonly Dictionary<Type, Type> OldNew = new()
        {
            { typeof(OldSB), typeof(NewSB) },
            { typeof(OldCheckable), typeof(NewCheckable) },
            { typeof(OldGrabbable), typeof(NewGrabbable) },
            { typeof(OldTriggerZone), typeof(NewTriggerZone) },
            { typeof(OldMultiEnabler), typeof(NewMultiEnabler) },
            { typeof(OldSteeringWheel), typeof(NewGrabbableLever) },
            { typeof(OldSnapZone), typeof(NewSnapZone) },
            { typeof(OldGoMultiEnabler), typeof(NewGoMultiEnabler) },
            { typeof(OldTeleportDestination), typeof(NewTeleportDestination) },
            { typeof(OldKeyhole), typeof(NewKeyhole) },
            { typeof(OldTriggerEvents), typeof(NewColliderProvider) },
            { typeof(OldToggleSwitch), typeof(NewToggleSwitch) },
            { typeof(OldWorldButton), typeof(NewWorldButton) },
            { typeof(OldRaycastable), typeof(NewRaycastable) },
            { typeof(OldGoTransformInteractable), typeof(NewGoTransformInteractable) },
        };

        private static void MultiEnabler(NewMultiEnabler dst, OldMultiEnabler src)
        {
            dst.enabled = src.enabled;
            dst.MonoBehaviours = src.MonoBehaviours.Select(Convert).ToList();
            return;

            MonoBehaviour Convert(MonoBehaviour oldBeh)
            {
                var oldType = oldBeh.GetType();
                if (!OldNew.TryGetValue(oldType, out var newType)) return oldBeh;
                var newBeh = (MonoBehaviour)oldBeh.GetComponent(newType);
                return newBeh ?? oldBeh;
            }
        }

        private static void MultiEnablerInverse(OldMultiEnabler dst, NewMultiEnabler src)
        {
            dst.enabled = src.enabled;
            dst.MonoBehaviours = src.MonoBehaviours.Select(ConvertInverse).ToList();
            return;

            MonoBehaviour ConvertInverse(MonoBehaviour newBeh)
            {
                var newType = newBeh.GetType();
                if (!OldNew.TryGetValue(newType, out var oldType)) return newBeh;
                var oldBeh = (MonoBehaviour)newBeh.GetComponent(oldType);
                return oldBeh ?? newBeh;
            }
        }

        private static void GoMultiEnablerInverse(OldGoMultiEnabler dst, NewGoMultiEnabler src)
        {
            dst.enabled = src.enabled;
            dst.GameObjects = src.GameObjects;
        }

        private static void GoMultiEnabler(NewGoMultiEnabler dst, OldGoMultiEnabler src)
        {
            dst.enabled = src.enabled;
            dst.GameObjects = src.GameObjects;
        }

        private static void SnapZoneConvert(SnapZone dst, OldSnapZone src)
        {
            dst.HeldItem = src.HeldItem?.GetComponent<NewGrabbable>();
            dst.StartingItem = src.StartingItem?.GetComponent<NewGrabbable>();
            dst.CanDropItem = src.CanDropItem;
            dst.CanSwapItem = src.CanSwapItem;
            dst.CanRemoveItem = src.CanRemoveItem;
            dst.ScaleItem = src.ScaleItem;
            dst.DisableColliders = src.DisableColliders;
            dst.DuplicateItemOnGrab = src.DuplicateItemOnGrab;
            dst.MaxDropTime = src.MaxDropTime;
        }

        private static void SnapZoneConvertInverse(OldSnapZone dst, SnapZone src)
        {
            dst.HeldItem = src.HeldItem?.GetComponent<OldGrabbable>();
            dst.StartingItem = src.StartingItem?.GetComponent<OldGrabbable>();
            dst.CanDropItem = src.CanDropItem;
            dst.CanSwapItem = src.CanSwapItem;
            dst.CanRemoveItem = src.CanRemoveItem;
            dst.ScaleItem = src.ScaleItem;
            dst.DisableColliders = src.DisableColliders;
            dst.DuplicateItemOnGrab = src.DuplicateItemOnGrab;
            dst.MaxDropTime = src.MaxDropTime;
        }

        private static void SteeringWheel(NewGrabbableLever dst, OldSteeringWheel src)
        {
            dst.MinAngle = src.MinAngle;
            dst.MaxAngle = src.MaxAngle;
            dst.RotatorObject = src.RotatorObject;
            dst.RotationSpeed = src.RotationSpeed;
            // dst.DebugText.text = src != null ? src.DebugText.text : null;
        }

        private static void SteeringWheelInverse(OldSteeringWheel dst, NewGrabbableLever src)
        {
            src.MinAngle = dst.MinAngle;
            src.MaxAngle = dst.MaxAngle;
            src.RotatorObject = dst.RotatorObject;
            src.RotationSpeed = dst.RotationSpeed;
            // src.DebugText.text = dst != null ? dst.DebugText.text : null;
        }

        private static void IndicationArrow(NewIndicationArrow dst, OldIndicationArrow src)
        {
            dst.MinAngle = src.MinAngle;
            dst.MaxAngle = src.MaxAngle;
            dst.MinValue = src.MinValue;
            dst.MaxValue = src.MaxValue;
            dst.AnglesAdjustmentCurve = src.AnglesAdjustmentCurve;
        }

        private static void IndicationArrowInverse(OldIndicationArrow dst, NewIndicationArrow src)
        {
            dst.MinAngle = src.MinAngle;
            dst.MaxAngle = src.MaxAngle;
            dst.MinValue = src.MinValue;
            dst.MaxValue = src.MaxValue;
            dst.AnglesAdjustmentCurve = src.AnglesAdjustmentCurve;
        }


        private static void GoTransformStates(NewGoTransformStates dst, OldGoTransformStates src)
        {
            dst.States = src.States.Select(oldState =>
                new NewTransformData(oldState.Postition, oldState.Rotation, oldState.Scale)).ToList();

            dst.SetState(src.State);

            dst.LerpSpeed = new Vector3(src.LerpSpeed, src.LerpSpeed, src.LerpSpeed);
            dst.LinearSpeed = new Vector3(src.LinearSpeed, src.LinearSpeed, src.LinearSpeed);
        }

        private static void GoTransformStatesInverse(OldGoTransformStates dst, NewGoTransformStates src)
        {
            dst.States = src.States
                .Select(newState => new OldTransformData(newState.Position, newState.Rotation, newState.Scale))
                .ToList();

            dst.SetState(src.State);

            dst.LerpSpeed = src.LerpSpeed.x;
            dst.LinearSpeed = src.LinearSpeed.x;
        }

        private static void TeleportDestination(NewTeleportDestination dst, OldTeleportDestination src) =>
            dst.DestinationTransform = src.DestinationTransform;

        private static void TeleportDestinationInverse(OldTeleportDestination dst, NewTeleportDestination src) =>
            dst.DestinationTransform = src.DestinationTransform;

        private static void Keyhole(NewKeyhole dst, OldKeyhole src)
        {
            dst.enabled = src.enabled;

            dst.Opened = src.Opened;
            dst.Closed = src.Closed;
            
            dst.SetTransforms(src.Rotator, src.FrontEntryPoint, src.BackEntryPoint);
            dst.SetThresholds(src.RotationThreshold, src.RotationThreshold, src.DistanceThreshold);
            dst.SetAngles(src.MinAngle, src.MaxAngle, src.OpenCloseAngleDelta);
            dst.SetItem(ItemConverter.instance.GetConfig(src.RequiredKeyConfig));
        }
        
        private static void KeyholeInverse(OldKeyhole dst, NewKeyhole src)
        {
            dst.enabled = src.enabled;

            dst.Opened = src.Opened;
            dst.Closed = src.Closed;
            
            dst.SetTransforms(src.Rotator, src.FrontEntryPoint, src.BackEntryPoint);
            dst.SetThresholds(src.ForwardRotationThreshold, src.DistanceThreshold);
            dst.SetAngles(src.MinAngle, src.MaxAngle, src.OpenCloseAngleDelta);
            dst.SetItem(ItemConverter.instance.GetItemSO(src.RequiredKeyConfig));
        }

        private static void ColliderProvider(NewColliderProvider dst, OldTriggerEvents src)
        {
            dst.enabled = src.enabled;

            dst.ProvideTriggers = !src.RejectTriggers;
            dst.FilterByTag = src.FilterByTag;
            if (dst.FilterByTag)
                dst.TagFilter = src.FilterTag;
        }
        
        private static void ColliderProviderInverse(OldTriggerEvents dst, NewColliderProvider src)
        {
            dst.enabled = src.enabled;

            dst.RejectTriggers = !src.ProvideTriggers;
            dst.FilterByTag = src.FilterByTag;
            if (dst.FilterByTag)
                dst.FilterTag = src.TagFilter;
        }

        private static void ToggleSwitch(NewToggleSwitch dst, OldToggleSwitch src)
        {
            dst.enabled = src.enabled;
            
            dst.DetectOnStay = src.DetectOnStay;

            try
            {
                dst.States = src.States.GetComponent<NewGoTransformStates>();
                dst.Trigger = src.Trigger.GetComponent<NewColliderProvider>();
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"{e.Message}. name: {dst.gameObject.name}");
            }
        }
        
        private static void ToggleSwitchInverse(OldToggleSwitch dst, NewToggleSwitch src)
        {
            dst.enabled = src.enabled;
            
            dst.DetectOnStay = src.DetectOnStay;

            try
            {
                dst.States = src.States.GetComponent<OldGoTransformStates>();
                dst.Trigger = src.Trigger.GetComponent<OldTriggerEvents>();
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"{e.Message}. name: {dst.gameObject.name}");
            }
        }

        private static void WorldButton(NewWorldButton dst, OldWorldButton src)
        {
            dst.enabled = src.enabled;

            try
            {
                dst.States = src.States.GetComponent<NewGoTransformStates>();
                dst.Trigger = src.Trigger.GetComponent<NewColliderProvider>();
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"{e.Message}. name: {dst.gameObject.name}");
            }
        }
        
        private static void WorldButtonInverse(OldWorldButton dst, NewWorldButton src)
        {
            dst.enabled = src.enabled;

            try
            {
                dst.States = src.States.GetComponent<OldGoTransformStates>();
                dst.Trigger = src.Trigger.GetComponent<OldTriggerEvents>();
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"{e.Message}. name: {dst.gameObject.name}");
            }
        }

        private static void Raycastable(NewRaycastable dst, OldRaycastable src)
        {
            dst.enabled = src.enabled;
        }
        
        private static void RaycastableInverse(OldRaycastable dst, NewRaycastable src)
        {
            dst.enabled = src.enabled;
        }
        
        private static void GoTransformInteractable(NewGoTransformInteractable dst, OldGoTransformInteractable src)
        {
            dst.enabled = src.enabled;
        }
        
        private static void GoTransformInteractableInverse(OldGoTransformInteractable dst, NewGoTransformInteractable src)
        {
            dst.enabled = src.enabled;
        }
    }
}