#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

//Checks if we are in a certain template, as some scripts are template-specific.
[InitializeOnLoad]
public static class TemplateEditorDetection {

    static TemplateEditorDetection() {

        //Current build target in NamedBuildTarget form
        var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup);

        //Get the current definition symbols
        string currentDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        List<string> allDefineSymbols = currentDefineSymbols
            .Split(';')
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        //Template core namespace classes used for detection
        var platformer = System.Type.GetType("Platformer.Core.Simulation", false);
        var kart = System.Type.GetType("KartGame.KartSystems.KartMovement", false);
        var ballgame = System.Type.GetType("TeamBallGame.Simulation", false);

        //Template definition symbols for use with #if
        var platformerDefine = "UNITY_TEMPLATE_PLATFORMER";
        var kartDefine = "UNITY_TEMPLATE_KART";
        var ballgameDefine = "UNITY_TEMPLATE_BALLGAME";

        //Add the define symbols if we are in a specific template, if they don't exist already
        bool changed = false;
        if (platformer != null && !allDefineSymbols.Contains(platformerDefine)) { allDefineSymbols.Add(platformerDefine); changed = true; }
        if (kart != null && !allDefineSymbols.Contains(kartDefine)) { allDefineSymbols.Add(kartDefine); changed = true; }
        if (ballgame != null && !allDefineSymbols.Contains(ballgameDefine)) { allDefineSymbols.Add(ballgameDefine); changed = true; }

        //Only apply if something changed, to avoid unnecessary recompiles
        if (changed) {
            PlayerSettings.SetScriptingDefineSymbols(
                namedBuildTarget,
                string.Join(";", allDefineSymbols));
        }
    }
}

#endif