using System;
using PopupIdentica.Core.Container;
using Scenario.Base.Components.Actions;
using Scenario.Base.Components.Conditions;
using Scenario.Core.Ecs.Components;
using Scenario.Core.Ecs.Systems;
using Scenario.Core.Ecs.VRTemplateEcs.Components;
using Scenario.Core.Model;
using UnityEngine;
using VRF.Scenario.Components.Actions;
using VRF.Scenario.Components.Conditions;
using VRF.Scenario.UI.ScenarioGame;
using VRF.VRBehaviours;
using OldKeyholeOpened = Scenario.Core.Ecs.Components.Conditions.KeyholeOpened;
using OldKeyhole = VRTemplate.VRBehaviours.Keys.Keyhole;
using NewKeyhole = VRF.VRBehaviours.Keys.Keyhole;
using OldKeyholeClosed = Scenario.Core.Ecs.Components.Conditions.KeyholeClosed;
using OldSB = Scenario.Core.ScenarioBehaviour;
using NewSB = Scenario.Core.World.ScenarioBehaviour;
using OldComponent = Scenario.Core.IScenarioComponent;
using NewComponent = Scenario.Core.Model.Interfaces.IScenarioComponent;
using OldProvider = Scenario.Core.ScenarioBehavioursProvider;
using NewProvider = Scenario.Core.World.ScenarioSceneProvider;
using OldCheckable = VRTemplate.Scripts.VRComponents.CheckableComponent;
using NewCheckable = VRF.VRBehaviours.Checking.Checkable;
using OldTriggerZone = Scenario.Core.Ecs.Utilities.TriggerZone;
using NewTriggerZone = Scenario.Utilities.TriggerZone;
using OldSnapZone = BNG.SnapZone;
using NewSnapZone = VRF.BNG_Framework.Scripts.Core.SnapZone;
using OldTeleportDestination = BNG.TeleportDestination;
using NewTeleportDestination = VRF.BNG_Framework.Scripts.Helpers.TeleportDestination;
using OldHingeJoint = BNG.HingeHelper;
using NewHingeJoint = VRF.BNG_Framework.Scripts.Core.HingeHelper;
using OldGrabbable = BNG.Grabbable;
using NewGrabbable = VRF.BNG_Framework.Scripts.Core.Grabbable;
using OldIndicationArrow = VRTemplate.Scripts.VRBehaviours.IndicationArrow;
using NewIndicationArrow = VRF.VRBehaviours.IndicationArrow;
using OldTimerPopup = PopupIdentica.Core.Container.PopupContainer;
using NewTimerPopup = VRF.Scenario.UI.Game.TimerScreen;
using OldGrabbaleLever = BNG.SteeringWheel;
using NewGrabbaleLever = VRF.VRBehaviours.GrabbableLever;
using OldTransformState = GOTransformStates;
using NewTransformState = VRF.VRBehaviours.GOTransformStates;
using SetTransformState = Scenario.Core.Ecs.VRTemplateEcs.Components.SetTransformState;

namespace LegacyCompatibilityPack2023.Converter.Content
{
    public static class ScenarioComponentCopyContent
    {
        public static NewComponent ConvertScenarioComponent(OldComponent old, NewProvider provider,
            ScenarioNode contextNode)
        {
            var isAction = contextNode is ActionNode;
            var isCondition = contextNode is ConditionNode;

            switch (old)
            {
                case GOActivityComponent goActivityComponent:
                    return new SetGameObjectActivity
                    {
                        GameObject = goActivityComponent.GameObject,
                        IsActive = goActivityComponent.IsActive,
                    };
                case AudioSourceComponent audioSourceComponent:
                    return new PlayAudio
                    {
                        AudioSource = audioSourceComponent.AudioSource,
                        AudioClip = audioSourceComponent.AudioClip,
                    };
                case CheckableECSComponent checkableEcsComponent:
                    return new CheckableChecked
                    {
                        Checkable = ConvertComponent<OldCheckable, NewCheckable>(checkableEcsComponent.CheckableEcs,
                            provider),
                    };
                case AnimatorComponent animatorComponent:
                    return new PlayAnimation
                    {
                        Animator = animatorComponent.Animator,
                        AnimationStateName = animatorComponent.AnimationStateName,
                        AnimationLayer = 0,
                        Loop = false,
                        Force = false,
                    };
                case MonoComponentActivityComponent monoComponentActivityComponent:
                    var mbOld = monoComponentActivityComponent.Component;
                    var mbNew = (MonoBehaviour)ConverterUtilities.GetNew(mbOld);
                    
                    return new SetMonoBehaviourActivity
                    {
                        MonoBehaviour = mbNew ? mbNew : mbOld,
                        IsActive = monoComponentActivityComponent.IsActive,
                    };
                case BehaviourActivityComponent behaviourActivityComponent:
                    var mb = behaviourActivityComponent.Behaviour as MonoBehaviour;
                    
                    return new SetMonoBehaviourActivity
                    {
                        MonoBehaviour = mb,
                        IsActive = behaviourActivityComponent.IsActive
                    };
                case SetTextDescriptionPopup setTextDescriptionPopup:
                    return new SetPopupDescription()
                    {
                        Popup = ConvertComponent<PopupContainer, PopupScreen>(setTextDescriptionPopup.PopupContainer,
                            provider),
                        Description = setTextDescriptionPopup.DescriptionText,
                    };
                case PopupClickedComponent popupClickedComponent:
                    return new PopupClicked
                    {
                        Popup = ConvertComponent<PopupContainer, PopupScreen>(popupClickedComponent.View, provider),
                    };
                case SetInfoTextComponent setInfoTextComponent:
                    return new SetInfoText
                    {
                        Text = setInfoTextComponent.Text,
                    };
                case ItemInTriggerComponent itemInTriggerComponent:
                    return new ItemEnteredTrigger
                    {
                        ItemType = ItemConverter.instance.GetConfig(itemInTriggerComponent.ItemType),
                        TriggerZone =
                            ConvertComponent<OldTriggerZone, NewTriggerZone>(itemInTriggerComponent.TriggerZone,
                                provider),
                    };

                case AnimationEndedComponent animationEndedComponent:
                    if (isAction)
                    {
                        return new Log()
                        {
                            Message = "AnimationEndedComponent is a condition, but used as action",
                            Type = ScenarioLogType.LogWarning,
                        };
                    }
                    return new AnimationEnded
                    {
                        Animator = animationEndedComponent.Animator,
                        AnimationStateName = animationEndedComponent.AnimationStateName,
                    };
                case SnapObjectToZoneComponent snapObjectToZoneComponent:
                    return new ItemSnapped // GrabbableSnapped
                    {
                        Zone = ConvertComponent<OldSnapZone, NewSnapZone>(snapObjectToZoneComponent.Zone, provider),
                        InventoryItemType =
                            ItemConverter.instance.GetConfig(snapObjectToZoneComponent.InventoryItemType),
                    };
                case SetPlayerPosition setPlayerPosition:
                    return new TeleportPlayer
                    {
                        Target = setPlayerPosition.Position,
                    };

                case PlayerInTriggerComponent playerInTriggerComponent:
                    return new PlayerEnteredTrigger()
                    {
                        TriggerZone =
                            ConvertComponent<OldTriggerZone, NewTriggerZone>(playerInTriggerComponent.TriggerZone,
                                provider),
                    };
                case AddItemInInventoryComponent addItemInInventoryComponent:
                    return new AddItem()
                    {
                        ItemConfig = ItemConverter.instance.GetConfig(addItemInInventoryComponent.InventoryItemSo)
                    };
                case TeleportDestinationComponent teleportDestinationComponent:
                    return new PlayerTeleported()
                    {
                        Destination =
                            ConvertComponent<OldTeleportDestination, NewTeleportDestination>(
                                teleportDestinationComponent.Destination, provider),
                    };
                case AudioEndedComponent audioEndedComponent:
                    return new AudioEnded()
                    {
                        AudioClip = audioEndedComponent.AudioClip
                    };
                case HingeJointAngleComponent hingeJointAngleComponent:
                    return new HingeJointAngleReached()
                    {
                        HingeHelper =
                            ConvertComponent<OldHingeJoint, NewHingeJoint>(hingeJointAngleComponent.HingeHelper,
                                provider),
                        Angle = hingeJointAngleComponent.Angle
                    };
                case TransformStateComponent transformStateComponent:
                    if (isAction)
                    {
                        return new VRF.Scenario.Components.Actions.SetTransformState
                        {
                            States = ConvertComponent<OldTransformState, NewTransformState>(
                                transformStateComponent.States, provider),
                            State = transformStateComponent.State
                        };
                    }

                    return new TransformStateReached
                    {
                        States = ConvertComponent<OldTransformState, NewTransformState>(
                            transformStateComponent.States, provider),
                        State = transformStateComponent.State
                    };
                case SetTransformState setTransformState:
                    return new VRF.Scenario.Components.Actions.SetTransformState
                    {
                        States = ConvertComponent<OldTransformState, NewTransformState>(setTransformState.States, provider),
                        State = setTransformState.State
                    };
                case StringTriggerComponent stringTriggerComponent:
                    if (isAction)
                    {
                        return new TriggerString
                        {
                            String = stringTriggerComponent.String
                        };
                    }

                    return new StringTriggered
                    {
                        String = stringTriggerComponent.String
                    };
                case UnsnapObjectFromZoneComponent unsnapObjectFromZoneComponent:
                    return new GrabbableUnsnapped()
                    {
                        Zone = ConvertComponent<OldSnapZone, NewSnapZone>(unsnapObjectFromZoneComponent.Zone, provider),
                        Grabbable = ConvertComponent<OldGrabbable, NewGrabbable>(
                            unsnapObjectFromZoneComponent.GrabbableObject, provider),
                    };
                case IndicationArrowComponent indicationArrowComponent:
                    return new SetArrowValue
                    {
                        IndicationArrow =
                            ConvertComponent<OldIndicationArrow, NewIndicationArrow>(
                                indicationArrowComponent.IndicationArrow, provider),
                        Value = indicationArrowComponent.Value,
                        Time = indicationArrowComponent.Time
                    };
                case DelayedTriggerComponent delayedTriggerComponent:
                    return new StartDelayTrigger
                    {
                        Name = delayedTriggerComponent.Name,
                        Seconds = delayedTriggerComponent.Seconds
                    };
                case DelayedTriggerCompletedComponent delayedTriggerCompletedComponent:
                    return new DelayTriggerEnded
                    {
                        Name = delayedTriggerCompletedComponent.Name
                    };
                case DeleteItemFromInventoryComponent deleteItemFromInventoryComponent:
                    return new DeleteItem
                    {
                        ItemConfig = ItemConverter.instance.GetConfig(deleteItemFromInventoryComponent.InventoryItemSo)
                    };
                case SetMaterialComponent setMaterialComponent:
                    return new SetMaterial
                    {
                        Renderer = setMaterialComponent.Renderer,
                        Index = setMaterialComponent.MaterialIndex,
                        Material = setMaterialComponent.Material
                    };
                case StartTimerComponent startTimerComponent:
                    return new StartTimer
                    {
                        ID = startTimerComponent.ID,
                        View = ConvertComponent<OldTimerPopup, NewTimerPopup>(startTimerComponent.TimerView, provider),
                        InGameTime = startTimerComponent.TargetTicks,
                        RealTime = startTimerComponent.TotalTime,
                        EnableOnStart = true,
                        DisableOnEnd = true,
                    };
                case TimerFinishComponent timerFinishComponent:
                    return new TimerEnded
                    {
                        ID = timerFinishComponent.ID,
                    };
                case SteeringWheelComponent steeringWheelComponent:
                    return new LeverSnapDegreeReached
                    {
                        GrabbableLever =
                            ConvertComponent<OldGrabbaleLever, NewGrabbaleLever>(steeringWheelComponent.SteeringWheel,
                                provider),
                        SnapDegreesIndex = steeringWheelComponent.SnapDegreesIndex,
                    };

                case SetScenarioModuleComponent setScenarioModuleComponent:
                    return new Log
                    {
                        Type = ScenarioLogType.LogError,
                        Message =
                            $"Replace this to {nameof(SetScenarioModuleComponent)}, Module={setScenarioModuleComponent.ModuleIndex}",
                    };
                case SetPathTargetComponent setPathTargetComponent:
                    return new Log
                    {
                        Type = ScenarioLogType.LogError,
                        Message =
                            $"Replace this to {nameof(SetPathTargetComponent)}, Target={setPathTargetComponent.Target?.name}",
                    };
                case MoveNextScenarioComponent moveNextScenarioComponent:
                    return new Log // StopScenario
                    {
                        Type = ScenarioLogType.Log,
                        Message = $"Move to next scenario",
                    };
                case StartItemsComponent startItemsComponent:
                    return new Log // AddItemInInventoryComponent
                    {
                        Type = ScenarioLogType.LogWarning,
                        Message = $"StartItemsComponent => AddItemInInventoryComponent",
                    };
                case ObjectGrabbedComponent objectGrabbedComponent:
                    return new GrabbableGrabbed
                    {
                        Value = ConvertComponent<OldGrabbable, NewGrabbable>(objectGrabbedComponent.Value, provider),
                    };
                case OldKeyholeOpened keyholeOpened:
                    return new KeyholeOpened
                    {
                        Keyhole = ConvertComponent<OldKeyhole, NewKeyhole>(keyholeOpened.Keyhole, provider)
                    };
                case OldKeyholeClosed keyholeClosed:
                    return new KeyholeClosed
                    {
                        Keyhole = ConvertComponent<OldKeyhole, NewKeyhole>(keyholeClosed.Keyhole, provider)
                    };
                case TMP_TextComponent tmpTextComponent:
                    return new SetTMPText
                    {
                        Text = tmpTextComponent.Text,
                        TMPText = tmpTextComponent.TMPText
                    };
                

                case TriggerClickedNearComponent:
                case IDComponent:
                case TransformStateListenerComponent transformStateListenerComponent:
                case PhysicalButtonDownComponent physicalButtonDownComponent:
                    return null;
            }

            // Если появляется эта ошибка - добавьте поддержку этого компонента сюда
            throw new Exception($"Old Component {old.GetType().Name} not found analog in New Components");
        }

        private static T2 ConvertComponent<T1, T2>(T1 old, NewProvider provider)
            where T1 : Component where T2 : Component
        {
            if (!old)
            {
                Debug.LogWarning($"Null OldComponent {typeof(T1).Name}");
                return null;
            }

            var oldSb = old.GetComponent<OldSB>();
            if (!oldSb)
            {
                Debug.LogWarning($"Can't find OldScenarioBehaviour for {old.GetType().Name}", old.gameObject);
                return null;
            }

            var id = oldSb.ID;
            var newObj = provider.Get(id);
            if (!newObj)
            {
                Debug.LogWarning($"Can't find NewScenarioBehaviour GameObject by ID {id}");
                return null;
            }

            var t2 = newObj.GetComponent<T2>();
            if (!newObj)
            {
                Debug.LogWarning($"Can't find NewComponent {typeof(T2).Name} for GameObject {newObj.name}", newObj);
            }

            return t2;
        }
    }
}