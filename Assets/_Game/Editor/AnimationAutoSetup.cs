#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public static class AnimationAutoSetup
{
    private const string AnimFolder = "Assets/_Game/Art/KayKitCharacters/Animations/Custom";
    private const string ControllerPath = "Assets/_Game/Data/PlayerAnimator_Warrior.controller";

    [MenuItem("GameTools/Setup Rig-Medium Animations")]
    public static void SetupAllAnimations()
    {
        Debug.Log("=== [AnimationAutoSetup] Bắt đầu tự động thiết lập Animation ===");

        if (!Directory.Exists(AnimFolder))
        {
            Directory.CreateDirectory(AnimFolder);
            AssetDatabase.Refresh();
        }

        // 1. Tạo 3 AnimationClip
        AnimationClip slideClip = CreateSlideClip();
        AnimationClip dodgeLeftClip = CreateDodgeClip(isLeft: true);
        AnimationClip dodgeRightClip = CreateDodgeClip(isLeft: false);

        // 2. Cập nhật Animator Controller
        SetupAnimatorController(slideClip, dodgeLeftClip, dodgeRightClip);

        // 3. Kiểm tra các Prefabs
        VerifyPrefabs();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("=== [AnimationAutoSetup] Hoàn tất thiết lập Animation thành công 100%! ===");
    }

    private static AnimationClip CreateSlideClip()
    {
        string path = $"{AnimFolder}/Anim_Slide.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip { name = "Anim_Slide" };
            AssetDatabase.CreateAsset(clip, path);
        }
        else
        {
            clip.ClearCurves();
        }

        clip.legacy = false;
        clip.frameRate = 30;

        float duration = 0.7f; // khớp với slideDuration trong PlayerController

        // Đường cong cho Hips (hạ thấp trọng tâm & ngửa người)
        AnimationCurve hipsPosY = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, -2.5f),
            new Keyframe(0.15f, -0.42f, 0f, 0f),
            new Keyframe(0.55f, -0.42f, 0f, 0f),
            new Keyframe(duration, 0f, 2.5f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips", typeof(Transform), "m_LocalPosition.y", hipsPosY);

        AnimationCurve hipsRotX = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 300f),
            new Keyframe(0.15f, 50f, 0f, 0f),
            new Keyframe(0.55f, 50f, 0f, 0f),
            new Keyframe(duration, 0f, -300f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips", typeof(Transform), "localEulerAnglesRaw.x", hipsRotX);

        // Cột sống (Spine) uốn cong về phía trước để giữ tầm nhìn
        AnimationCurve spineRotX = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, -150f),
            new Keyframe(0.15f, -25f, 0f, 0f),
            new Keyframe(0.55f, -25f, 0f, 0f),
            new Keyframe(duration, 0f, 150f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips/spine", typeof(Transform), "localEulerAnglesRaw.x", spineRotX);

        // Chân trái (Left Leg) duỗi thẳng trượt tới trước
        AnimationCurve legLRotX = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, -350f),
            new Keyframe(0.15f, -55f, 0f, 0f),
            new Keyframe(0.55f, -55f, 0f, 0f),
            new Keyframe(duration, 0f, 350f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips/upperleg.l", typeof(Transform), "localEulerAnglesRaw.x", legLRotX);

        AnimationCurve lowerLegLRotX = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 100f),
            new Keyframe(0.15f, 15f, 0f, 0f),
            new Keyframe(0.55f, 15f, 0f, 0f),
            new Keyframe(duration, 0f, -100f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips/upperleg.l/lowerleg.l", typeof(Transform), "localEulerAnglesRaw.x", lowerLegLRotX);

        // Chân phải (Right Leg) gập lại sát hông
        AnimationCurve legRRotX = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 250f),
            new Keyframe(0.15f, 40f, 0f, 0f),
            new Keyframe(0.55f, 40f, 0f, 0f),
            new Keyframe(duration, 0f, -250f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips/upperleg.r", typeof(Transform), "localEulerAnglesRaw.x", legRRotX);

        AnimationCurve lowerLegRRotX = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, -400f),
            new Keyframe(0.15f, -65f, 0f, 0f),
            new Keyframe(0.55f, -65f, 0f, 0f),
            new Keyframe(duration, 0f, 400f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips/upperleg.r/lowerleg.r", typeof(Transform), "localEulerAnglesRaw.x", lowerLegRRotX);

        // Hai tay đưa ra sau giữ thăng bằng
        AnimationCurve armRotX = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 250f),
            new Keyframe(0.15f, 45f, 0f, 0f),
            new Keyframe(0.55f, 45f, 0f, 0f),
            new Keyframe(duration, 0f, -250f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips/spine/chest/upperarm.l", typeof(Transform), "localEulerAnglesRaw.x", armRotX);
        clip.SetCurve("Rig_Medium/root/hips/spine/chest/upperarm.r", typeof(Transform), "localEulerAnglesRaw.x", armRotX);

        EditorUtility.SetDirty(clip);
        Debug.Log("[AnimationAutoSetup] Đã tạo clip: " + path);
        return clip;
    }

    private static AnimationClip CreateDodgeClip(bool isLeft)
    {
        string name = isLeft ? "Anim_Dodge_Left" : "Anim_Dodge_Right";
        string path = $"{AnimFolder}/{name}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip { name = name };
            AssetDatabase.CreateAsset(clip, path);
        }
        else
        {
            clip.ClearCurves();
        }

        clip.legacy = false;
        clip.frameRate = 30;

        float duration = 0.3f;
        float dir = isLeft ? -1f : 1f;

        // Nghiêng hông và cột sống (Roll Z)
        AnimationCurve hipsRotZ = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, dir * 150f),
            new Keyframe(0.12f, dir * 18f, 0f, 0f),
            new Keyframe(duration, 0f, -dir * 150f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips", typeof(Transform), "localEulerAnglesRaw.z", hipsRotZ);

        // Xoay nhẹ theo hướng né (Yaw Y)
        AnimationCurve hipsRotY = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, dir * 100f),
            new Keyframe(0.10f, dir * 12f, 0f, 0f),
            new Keyframe(duration, 0f, -dir * 100f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips", typeof(Transform), "localEulerAnglesRaw.y", hipsRotY);

        // Cột sống nghiêng bù trừ cho tự nhiên
        AnimationCurve spineRotZ = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, dir * 80f),
            new Keyframe(0.12f, dir * 8f, 0f, 0f),
            new Keyframe(duration, 0f, -dir * 80f, 0f)
        );
        clip.SetCurve("Rig_Medium/root/hips/spine", typeof(Transform), "localEulerAnglesRaw.z", spineRotZ);

        EditorUtility.SetDirty(clip);
        Debug.Log("[AnimationAutoSetup] Đã tạo clip: " + path);
        return clip;
    }

    private static void SetupAnimatorController(AnimationClip slideClip, AnimationClip dodgeLeftClip, AnimationClip dodgeRightClip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError("[AnimationAutoSetup] Không tìm thấy AnimatorController tại " + ControllerPath);
            return;
        }

        // Đảm bảo đủ parameters
        AddParameterIfNotExists(controller, "SlideTrigger", AnimatorControllerParameterType.Trigger);
        AddParameterIfNotExists(controller, "DodgeLeftTrigger", AnimatorControllerParameterType.Trigger);
        AddParameterIfNotExists(controller, "DodgeRightTrigger", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        // Tìm State Run & Jump
        AnimatorState runState = null;
        AnimatorState jumpState = null;
        foreach (var childState in stateMachine.states)
        {
            if (childState.state.name == "Run") runState = childState.state;
            else if (childState.state.name == "Jump") jumpState = childState.state;
        }

        if (runState == null)
        {
            Debug.LogError("[AnimationAutoSetup] Không tìm thấy state Run trong controller!");
            return;
        }

        // Tạo hoặc lấy State Slide
        AnimatorState slideState = GetOrCreateState(stateMachine, "Slide", slideClip, new Vector3(450, 50, 0));
        slideState.motion = slideClip;

        // Tạo hoặc lấy State DodgeLeft
        AnimatorState dodgeLeftState = GetOrCreateState(stateMachine, "DodgeLeft", dodgeLeftClip, new Vector3(450, 150, 0));
        dodgeLeftState.motion = dodgeLeftClip;

        // Tạo hoặc lấy State DodgeRight
        AnimatorState dodgeRightState = GetOrCreateState(stateMachine, "DodgeRight", dodgeRightClip, new Vector3(450, 250, 0));
        dodgeRightState.motion = dodgeRightClip;

        // Nối transitions
        // 1. Run -> Slide
        AddTriggerTransition(runState, slideState, "SlideTrigger", 0.1f);
        // Slide -> Run khi kết thúc clip
        AddExitTimeTransition(slideState, runState, 0.85f, 0.15f);

        // Jump -> Slide (cho phép trượt khi đang rơi tiếp đất)
        if (jumpState != null)
        {
            AddTriggerTransition(jumpState, slideState, "SlideTrigger", 0.1f);
        }

        // 2. Run -> DodgeLeft
        AddTriggerTransition(runState, dodgeLeftState, "DodgeLeftTrigger", 0.08f);
        AddExitTimeTransition(dodgeLeftState, runState, 0.85f, 0.12f);

        // 3. Run -> DodgeRight
        AddTriggerTransition(runState, dodgeRightState, "DodgeRightTrigger", 0.08f);
        AddExitTimeTransition(dodgeRightState, runState, 0.85f, 0.12f);

        EditorUtility.SetDirty(controller);
        Debug.Log("[AnimationAutoSetup] Đã cập nhật AnimatorController thành công.");
    }

    private static void AddParameterIfNotExists(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        foreach (var p in controller.parameters)
        {
            if (p.name == name) return;
        }
        controller.AddParameter(name, type);
    }

    private static AnimatorState GetOrCreateState(AnimatorStateMachine sm, string name, Motion motion, Vector3 pos)
    {
        foreach (var cs in sm.states)
        {
            if (cs.state.name == name) return cs.state;
        }
        AnimatorState state = sm.AddState(name, pos);
        state.motion = motion;
        return state;
    }

    private static void AddTriggerTransition(AnimatorState from, AnimatorState to, string triggerName, float duration)
    {
        // Kiểm tra xem transition đã tồn tại chưa
        foreach (var t in from.transitions)
        {
            if (t.destinationState == to)
            {
                foreach (var c in t.conditions)
                {
                    if (c.parameter == triggerName) return;
                }
            }
        }

        var transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = duration;
        transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
    }

    private static void AddExitTimeTransition(AnimatorState from, AnimatorState to, float exitTime, float duration)
    {
        foreach (var t in from.transitions)
        {
            if (t.destinationState == to && t.hasExitTime) return;
        }

        var transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = exitTime;
        transition.hasFixedDuration = true;
        transition.duration = duration;
    }

    private static void VerifyPrefabs()
    {
        string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game/Prefabs/Player" });
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);

        foreach (string guid in prefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            Animator anim = prefab.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                if (anim.runtimeAnimatorController == null && controller != null)
                {
                    anim.runtimeAnimatorController = controller;
                    EditorUtility.SetDirty(prefab);
                    Debug.Log($"[AnimationAutoSetup] Đã gán Controller cho prefab: {path}");
                }
            }
        }
    }
}
#endif
