using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectMusic;
using GUZ.Core.Context;
using GUZ.Core.Creator.Sounds;
using GUZ.Core.Data;
using JetBrains.Annotations;
using UnityEngine;
using ZenKit;
using Font = ZenKit.Font;
using Mesh = ZenKit.Mesh;
using Object = UnityEngine.Object;
using Texture = ZenKit.Texture;

namespace GUZ.Core
{
	public static class ResourceLoader
	{
		private static readonly Vfs Vfs = new();
		private static readonly Loader DmLoader = Loader.Create(LoaderOptions.Default | LoaderOptions.Download);

		private static readonly Resource<ITexture> Textures = new(
			s => new Texture(Vfs, s).Cache()
		);

		private static readonly Resource<IModelScript> ModelScript = new(
			s => new ModelScript(Vfs, s).Cache()
		);

		private static readonly Resource<IModelAnimation> ModelAnimation = new(
			s => new ModelAnimation(Vfs, s).Cache()
		);

		private static readonly Resource<IMesh> Mesh = new(
			s => new Mesh(Vfs, s).Cache()
		);

		private static readonly Resource<IModelHierarchy> ModelHierarchy = new(
			s => new ModelHierarchy(Vfs, s).Cache()
		);

		private static readonly Resource<IModel> Model = new(
			s => new Model(Vfs, s).Cache()
		);

		private static readonly Resource<IModelMesh> ModelMesh = new(
			s => new ModelMesh(Vfs, s).Cache()
		);

		private static readonly Resource<IMultiResolutionMesh> MultiResolutionMesh = new(
			s => new MultiResolutionMesh(Vfs, s).Cache()
		);

		private static readonly Resource<IMorphMesh> MorphMesh = new(
			s => new MorphMesh(Vfs, s).Cache()
		);

		private static readonly Resource<IFont> Font = new(
			s => new Font(Vfs, s).Cache()
		);
		
		private static readonly Resource<SoundData> Sound = new(
			s =>
			{
				var node = Vfs.Find(s);
				return node == null ? null : SoundCreator.ConvertWavByteArrayToFloatArray(node.Buffer.Bytes);
			}
		);

		private static readonly Resource<GameObject> Prefab = new(s =>
		{
			// Lookup is done in following places:
			// 1. CONTEXT_NAME/Prefabs/... - overwrites lookup path below, used for specific prefabs, for current context (HVR, Flat, ...)
			// 2. Prefabs/... - Located inside core module (GVR), if we don't need special handling.
            var contextPrefixPath = $"{GUZContext.InteractionAdapter.GetContextName()}/{s}";
            return new[] { contextPrefixPath, s }.Select(Resources.Load<GameObject>).FirstOrDefault(newPrefab => newPrefab != null);
		});

		public static void Init(string root)
		{
			var workPath = FindWorkPath(root);
			var diskPaths = FindDiskPaths(root);

			diskPaths.ForEach(v => Vfs.MountDisk(v, VfsOverwriteBehavior.Newer));
			Vfs.Mount(Path.GetFullPath(workPath), "/_work", VfsOverwriteBehavior.All);
			
            DmLoader.AddResolver(name =>
            {
				var node = Vfs.Find(name);
				return node?.Buffer.Bytes;
            });
		}

		[CanBeNull]
		public static ITexture TryGetTexture([NotNull] string key)
		{
			return Textures.TryLoad($"{GetPreparedKey(key)}-c.tex", out var item) ? item : null;
		}

		[CanBeNull]
		public static IModelScript TryGetModelScript([NotNull] string key)
		{
			return ModelScript.TryLoad($"{GetPreparedKey(key)}.mds", out var item) ? item : null;
		}

		[CanBeNull]
		public static IModelAnimation TryGetModelAnimation([NotNull] string mds, [NotNull] string key)
		{
			key = $"{GetPreparedKey(mds)}-{GetPreparedKey(key)}.man";
			return ModelAnimation.TryLoad(key, out var item) ? item : null;
		}

		[CanBeNull]
		public static IMesh TryGetMesh([NotNull] string key)
		{
			return Mesh.TryLoad($"{GetPreparedKey(key)}.msh", out var item) ? item : null;
		}

		[CanBeNull]
		public static IModelHierarchy TryGetModelHierarchy([NotNull] string key)
		{
			return ModelHierarchy.TryLoad($"{GetPreparedKey(key)}.mdh", out var item) ? item : null;
		}

		[CanBeNull]
		public static IModel TryGetModel([NotNull] string key)
		{
			return Model.TryLoad($"{GetPreparedKey(key)}.mdl", out var item) ? item : null;
		}

		[CanBeNull]
		public static IModelMesh TryGetModelMesh([NotNull] string key)
		{
			return ModelMesh.TryLoad($"{GetPreparedKey(key)}.mdm", out var item) ? item : null;
		}

		[CanBeNull]
		public static IMultiResolutionMesh TryGetMultiResolutionMesh([NotNull] string key)
		{
			return MultiResolutionMesh.TryLoad($"{GetPreparedKey(key)}.mrm", out var item) ? item : null;
		}

		[CanBeNull]
		public static IMorphMesh TryGetMorphMesh([NotNull] string key)
		{
			return MorphMesh.TryLoad($"{GetPreparedKey(key)}.mmb", out var item) ? item : null;
		}

		[CanBeNull]
		public static IFont TryGetFont([NotNull] string key)
		{
			return Font.TryLoad($"{GetPreparedKey(key)}.fnt", out var item) ? item : null;
		}

		[CanBeNull]
		public static SoundData TryGetSound([NotNull] string key)
		{
			return Sound.TryLoad($"{GetPreparedKey(key)}.wav", out var item) ? item : null;
		}
		
		[CanBeNull]
		public static Segment TryGetSegment([NotNull] string key)
		{
			// NOTE(lmichaelis): There is no caching required here, since the loader
			//                   already caches segments upon loading them
			return DmLoader.GetSegment(key);
		}
		
		[CanBeNull]
		public static DaedalusVm TryGetDaedalusVm([NotNull] string key)
		{
			// NOTE(lmichaelis): These are not cached, since they contain internal state
			//                   which should not be shared.
			return new DaedalusVm(Vfs, $"{GetPreparedKey(key)}.dat");
		}

		[CanBeNull]
		public static ZenKit.World TryGetWorld([NotNull] string key)
		{
			return new ZenKit.World(Vfs, $"{GetPreparedKey(key)}.zen");
		}
		
		[CanBeNull]
		public static ZenKit.World TryGetWorld([NotNull] string key, GameVersion version)
		{
			return new ZenKit.World(Vfs, $"{GetPreparedKey(key)}.zen", version);
		}

		[CanBeNull]
		public static GameObject TryGetPrefab(PrefabType key)
		{
			return Prefab.TryLoad(key.Path(), out var item) ? item : null;
		}

		[CanBeNull]
		public static GameObject TryGetPrefabObject(PrefabType key)
		{
			return Object.Instantiate(TryGetPrefab(key));
		}

		[NotNull]
		private static string GetPreparedKey([NotNull] string key)
		{
			var extension = Path.GetExtension(key);
			var withoutExtension = extension == string.Empty ? key : key.Replace(extension, "");
			return withoutExtension.ToLower();
		}


		private static string FindWorkPath(string root)
		{
			var path = Directory.GetDirectories(root, "_work", new EnumerationOptions
			{
				MatchCasing = MatchCasing.CaseInsensitive,
				RecurseSubdirectories = false
			}).First();

			return Path.GetFullPath(path, root);
		}

		private static List<string> FindDiskPaths(string root)
		{
			var path = Directory.GetDirectories(root, "data", new EnumerationOptions
			{
				MatchCasing = MatchCasing.CaseInsensitive,
				RecurseSubdirectories = false
			}).First();

			var data = Path.GetFullPath(path, root);
			var files = Directory.GetFiles(data, "*.vdf", new EnumerationOptions
			{
				MatchCasing = MatchCasing.CaseInsensitive,
				RecurseSubdirectories = true,
				IgnoreInaccessible = true
			});

			return files.Select(v => Path.GetFullPath(v, data)).ToList();
		}
	}
}