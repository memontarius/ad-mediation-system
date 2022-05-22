#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;

public class CBPreprocess : IPreprocessBuild
{
	 public int callbackOrder { get { return 0; } }
		 public void OnPreprocessBuild(BuildTarget target, string path) {
		 CBCleanup.Clean();

		 }
}
#endif
