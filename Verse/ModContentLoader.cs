using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RimWorld.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Verse
{
	public static class ModContentLoader<T> where T : class
	{
		private static string[] AcceptableExtensionsAudio = new string[7]
		{
			".wav",
			".mp3",
			".ogg",
			".xm",
			".it",
			".mod",
			".s3m"
		};

		private static string[] AcceptableExtensionsTexture = new string[4]
		{
			".png",
			".jpg",
			".jpeg",
			".psd"
		};

		private static string[] AcceptableExtensionsString = new string[1]
		{
			".txt"
		};

		public static bool IsAcceptableExtension(string extension)
		{
			string[] array;
			if (typeof(T) == typeof(AudioClip))
			{
				array = AcceptableExtensionsAudio;
			}
			else if (typeof(T) == typeof(Texture2D))
			{
				array = AcceptableExtensionsTexture;
			}
			else
			{
				if (!(typeof(T) == typeof(string)))
				{
					Log.Error("Unknown content type " + typeof(T));
					return false;
				}
				array = AcceptableExtensionsString;
			}
			string[] array2 = array;
			foreach (string b in array2)
			{
				if (extension.ToLower() == b)
				{
					return true;
				}
			}
			return false;
		}

		public static IEnumerable<Pair<string, LoadedContentItem<T>>> LoadAllForMod(ModContentPack mod)
		{
			DeepProfiler.Start(string.Concat("Loading assets of type ", typeof(T), " for mod ", mod));
			Dictionary<string, FileInfo> allFilesForMod = ModContentPack.GetAllFilesForMod(mod, GenFilePaths.ContentPath<T>(), IsAcceptableExtension);
			foreach (KeyValuePair<string, FileInfo> item in allFilesForMod)
			{
				LoadedContentItem<T> loadedContentItem = LoadItem((FilesystemFile)item.Value);
				if (loadedContentItem != null)
				{
					yield return new Pair<string, LoadedContentItem<T>>(item.Key, loadedContentItem);
				}
			}
			DeepProfiler.End();
		}

		public static LoadedContentItem<T> LoadItem(VirtualFile file)
		{
			try
			{
				if (typeof(T) == typeof(string))
				{
					return new LoadedContentItem<T>(file, (T)(object)file.ReadAllText());
				}
				if (typeof(T) == typeof(Texture2D))
				{
					return new LoadedContentItem<T>(file, (T)(object)LoadTexture(file));
				}
				if (typeof(T) == typeof(AudioClip))
				{
					if (Prefs.LogVerbose)
					{
						DeepProfiler.Start("Loading file " + file);
					}
					IDisposable extraDisposable = null;
					T val;
					try
					{
						string uri = GenFilePaths.SafeURIForUnityWWWFromPath(file.FullPath);
						using UnityWebRequest unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(uri, GetAudioTypeFromURI(uri));
						unityWebRequest.SendWebRequest();
						while (!unityWebRequest.isDone)
						{
							Thread.Sleep(1);
						}
						if (unityWebRequest.error != null)
						{
							throw new InvalidOperationException(unityWebRequest.error);
						}
						val = (T)(object)DownloadHandlerAudioClip.GetContent(unityWebRequest);
					}
					finally
					{
						if (Prefs.LogVerbose)
						{
							DeepProfiler.End();
						}
					}
					UnityEngine.Object @object = val as UnityEngine.Object;
					if (@object != null)
					{
						@object.name = Path.GetFileNameWithoutExtension(file.Name);
					}
					return new LoadedContentItem<T>(file, val, extraDisposable);
				}
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat("Exception loading ", typeof(T), " from file.\nabsFilePath: ", file.FullPath, "\nException: ", ex.ToString()));
			}
			if (typeof(T) == typeof(Texture2D))
			{
				return (LoadedContentItem<T>)(object)new LoadedContentItem<Texture2D>(file, BaseContent.BadTex);
			}
			return null;
		}

		private static AudioType GetAudioTypeFromURI(string uri)
		{
			if (uri.EndsWith(".ogg"))
			{
				return AudioType.OGGVORBIS;
			}
			return AudioType.WAV;
		}

		private static bool ShouldStreamAudioClipFromFile(VirtualFile file)
		{
			if (!(file is FilesystemFile) || !file.Exists)
			{
				return false;
			}
			return file.Length > 307200;
		}

		private static Texture2D LoadTexture(VirtualFile file)
		{
			Texture2D texture2D = null;
			if (file.Exists)
			{
				byte[] data = file.ReadAllBytes();
				texture2D = new Texture2D(2, 2, TextureFormat.Alpha8, mipChain: true);
				texture2D.LoadImage(data);
				texture2D.Compress(highQuality: true);
				texture2D.name = Path.GetFileNameWithoutExtension(file.Name);
				texture2D.filterMode = FilterMode.Trilinear;
				texture2D.anisoLevel = 2;
				texture2D.Apply(updateMipmaps: true, makeNoLongerReadable: true);
			}
			return texture2D;
		}
	}
}
