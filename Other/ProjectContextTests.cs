using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using VRF.Editor.TestBase;
using VRF.Identities;
using VRF.Identities.Core;
using VRF.Inventory.Installers;
using VRF.Inventory.Scriptables;
using VRF.Networking.Core;
using VRF.Scenes.Project;
using VRF.Scenes.Scriptables;
using VRF.Utilities.Behaviours;
using Zenject;

namespace VRF.Tests.EditMode
{
    /// <summary>
    /// Тесты для проверки правильности использования ProjectContext в проекте и с VRF в частности
    /// </summary>
    public class ProjectContextTests : BaseZenjectContextTests
    {
        private const string DefaultProjectContextPath = "Assets/Resources/ProjectContext.prefab";
        
        [Test]
        [Category(TestMetaData.Category.Files)]
        [Author(TestMetaData.Author.Vertoker)]
        [Description("Существует ли ProjectContext в заготовленном для него месте?")]
        public void ExistsProjectContext()
        {
            var contexts = FindComponents<ProjectContext>().ToArray();
            if (contexts.Length == 0)
                LogError($"Can't find {nameof(ProjectContext)} in project, " +
                         $"add prefab to the {DefaultProjectContextPath}");
            
            var paths = contexts.Select(AssetDatabase.GetAssetPath);
            if (paths.All(p => p != DefaultProjectContextPath))
                LogError($"{nameof(ProjectContext)} can't find in {DefaultProjectContextPath}, " +
                         $"add specifically to this path");
            
            ThrowIfError<FileNotFoundException>();
        }
        
        [Test]
        [Category(TestMetaData.Category.ProjectContext)]
        [Author(TestMetaData.Author.Vertoker)]
        [Description("Проверка инсталлеров внутри ProjectContext на null")]
        public void CheckProjectContext()
        {
            var context = FindComponents<ProjectContext>().First();
            
            if (!context.TryGetComponent<ProjectContextExtensions>(out _))
                LogError($"Add {nameof(ProjectContextExtensions)} to the root and press ValidateInstallers");
            
            var countNull = context.Installers.Count(i => !i);
            if (countNull > 0)
                LogError($"{nameof(ProjectContext)} contains {countNull} null reference installers");
            
            var installers = context.GetComponentsInChildren<MonoInstaller>();
            int l1 = installers.Length, l2 = context.Installers.Count();
            if (l1 != l2)
                LogError($"{nameof(ProjectContext)} installers doesn't reference all installers in self, " +
                         $"real installers - {l1}, in context - {l2}. " +
                         $"Press ValidateInstallers in {nameof(ProjectContextExtensions)}");
            
            ThrowIfError<NullReferenceException>();
        }
        
        [Test]
        [Category(TestMetaData.Category.ProjectInstallers)]
        [Author(TestMetaData.Author.Vertoker)]
        public void CheckScenes()
        {
            var installer = GetInstaller<ScenesServiceInstaller>(GetProjectContext());
            if (installer == null || !installer.gameObject.activeInHierarchy) return;
            
            if (!installer.ScenesConfig)
                LogError(NullInstallerConfigMessage<ScenesServiceInstaller, ClientScenesConfig>(), installer);
            
            ThrowIfError<NullReferenceException>();
        }
        
        [Test]
        [Category(TestMetaData.Category.ProjectInstallers)]
        [Author(TestMetaData.Author.Vertoker)]
        public void CheckInventory()
        {
            var installer = GetInstaller<InventoryProjectInstaller>(GetProjectContext());
            if (installer == null || !installer.gameObject.activeInHierarchy) return;
            
            if (!installer.ProjectConfig)
                LogError(NullInstallerConfigMessage<InventoryProjectInstaller, InventoryProjectConfig>(), installer);
            if (!installer.PoolConfig)
                LogError(NullInstallerConfigMessage<InventoryProjectInstaller, InventoryItemMainList>(), installer);
            
            ThrowIfError<NullReferenceException>();
        }
        
        [Test]
        [Category(TestMetaData.Category.ProjectInstallers)]
        [Author(TestMetaData.Author.Vertoker)]
        public void CheckIdentities()
        {
            var installer = GetInstaller<IdentityInstaller>(GetProjectContext());
            if (installer == null || !installer.gameObject.activeInHierarchy) return;
            
            if (!installer.Identities)
                LogError(NullInstallerConfigMessage<IdentityInstaller, IdentitiesConfig>(), installer);
            
            ThrowIfError<NullReferenceException>();
        }
        
        [Test]
        [Category(TestMetaData.Category.ProjectInstallers)]
        [Author(TestMetaData.Author.Vertoker)]
        public void CheckWarningsSettings()
        {
            var context = GetProjectContext();
            if (!context) return;
            
            if (context.Settings.DisplayWarningWhenResolvingDuringInstall)
                LogError($"Disable {nameof(context.Settings.DisplayWarningWhenResolvingDuringInstall)} " +
                         $"in ProjectContext Settings", context.gameObject);
            if (context.Settings.Signals.MissingHandlerDefaultResponse != SignalMissingHandlerResponses.Ignore)
                LogError($"Set {nameof(context.Settings.Signals)}." +
                         $"{nameof(context.Settings.Signals.MissingHandlerDefaultResponse)} to " +
                         $"{nameof(SignalMissingHandlerResponses.Ignore)} " +
                         $"in ProjectContext Settings", context.gameObject);
            
            ThrowIfError<ArgumentException>();
        }
    }
}