# NumpySkeletonVisulizer
Unity project for visualizing skeleton(.npy file)

I made this project for 3D Human Pose Estimation Task.

You can load a __.npy__ file(not .npz) with shape (T, J, 2 or 3).
After load a file, you can see J sphere objects moving according to the loaded array.

Click the "Connection Joints" button to pause the skeleton animation. You can then click on one joint, then hold down the mouse and drag to another joint to make it a single skeleton. It is a LineRenderer that lasts about 1 to 2 frame times (Time.deltaTime). 

If you want to remove unnecessary object creation and destruction, and use this program only in the scene view of the Unity editor, not the camera view, you can replace DrawLine with Debug.DrawLine.

- There are some SerializedFields such as joint scale(default 1) or skeleton length(default 30) scale that you can adjust the value of.
- There is a npy file(Asset/Data/test_data.npy) which is the pose squence data of Human3.6M/S1/Eating.


##### TODO
- function to adjust the length of __each__ skeleton.
- function to render lines by connecting each joint by the user.
- support .npz file

This project uses additional packages: NuGetForUnity and NumSharp.
