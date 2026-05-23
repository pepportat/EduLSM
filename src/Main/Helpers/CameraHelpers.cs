using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;

namespace Main.Helpers;

public static class CameraHelpers
{
    public static void HandleCameraZoom(ref Camera2D camera, float delta, float zoomScale = 0.2f, float zoomMin = 0.1f, float zoomMax = 10.0f)
    {
        var mouseWorldPos = GetScreenToWorld2D(GetMousePosition(), camera);

        camera.Offset = GetMousePosition();

        camera.Target = mouseWorldPos;

        var scale = zoomScale * delta;
        camera.Zoom = Clamp((float)Math.Exp(Math.Log(camera.Zoom) + scale), zoomMin, zoomMax);
    }

    public static void HandleCameraPan(ref Camera2D camera)
    {
        var delta = GetMouseDelta();

        delta = Vector2Scale(delta, -1.0f / camera.Zoom);
        camera.Target = Vector2Add(camera.Target, delta);
    }
}