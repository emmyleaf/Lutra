using System.Collections.Generic;
using System.Numerics;

namespace Lutra.Cameras
{
    public class CameraManager
    {
        private Stack<Camera> CameraStack = new();

        public CameraManager(float InitialViewportWidth, float InitialViewportHeight)
        {
            // Create default camera.
            CameraStack.Push(new Camera(InitialViewportWidth / 2, InitialViewportHeight / 2, InitialViewportWidth, InitialViewportHeight));
        }

        public void PushCamera(Camera camera)
        {
            CameraStack.Push(camera);
        }

        public bool RemoveCameraAndAbove(Camera camera)
        {
            if (CameraStack.Contains(camera))
            {
                Camera popped = null;
                while (popped != camera)
                {
                    popped = CameraStack.Pop();
                }
                return true;
            }

            return false;
        }

        public Camera PopCamera()
        {
            if (CameraStack.Count > 1)
            {
                return CameraStack.Pop();
            }

            return null;
        }

        public void ClearStack()
        {
            while (PopCamera() != null) { } // ???
        }

        public void ForceSwitchCamera(Camera camera)
        {
            ClearStack();
            PushCamera(camera);
        }

        public int CameraCount => CameraStack.Count;
        public Camera ActiveCamera => CameraStack.Peek();
        public Matrix4x4 View => CameraStack.Peek().View;
        public Matrix4x4 Projection => CameraStack.Peek().Projection;
    }
}
