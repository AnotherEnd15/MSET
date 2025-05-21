using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Foo
{
    public Foo()
    {
        PlayerSettings.WebGL.emscriptenArgs = "-Wl,--trace-symbol=sendfile";
    }
}
