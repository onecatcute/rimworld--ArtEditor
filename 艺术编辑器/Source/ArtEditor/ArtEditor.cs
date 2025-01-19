using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArtEditor
{
    [StaticConstructorOnStartup] // 使用此特性确保类在程序加载时初始化
    public static class Startup
    {
        static Startup() => new Harmony("ArtEditor").PatchAll(); // 使用 Harmony 工具补丁所有相关代码
    }

    public class Dialog_EditArtTitle : Window // 定义一个窗口类，用于编辑艺术品标题
    {
        private readonly CompArt compArt; // 存储艺术组件的引用
        private string newTitle; // 用于存储用户输入的新标题

        public Dialog_EditArtTitle(CompArt compArt) // 构造函数，用于初始化窗口
        {
            this.compArt = compArt; // 将传入的艺术组件赋值给类变量
            this.newTitle = compArt.Title; // 初始化输入框中的默认标题为当前标题
            this.doCloseButton = false; // 禁用窗口的底部关闭按钮
            this.doCloseX = true; // 启用窗口右上角的关闭按钮
            this.absorbInputAroundWindow = true; // 吸收窗口周围的用户输入，避免误操作
            this.forcePause = true; // 打开窗口时暂停游戏
        }

        public override Vector2 InitialSize => new Vector2(500f, 165f); // 设置窗口的初始尺寸

        public override void OnAcceptKeyPressed() => Event.current.Use(); // 禁止默认的确认按键行为，避免窗口误关闭

        public override void DoWindowContents(Rect inRect) // 重写方法，绘制窗口内容
        {
            Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "输入新的标题".Translate()); // 在窗口顶部显示标题输入提示
            newTitle = Widgets.TextField(new Rect(0f, 40f, inRect.width, 30f), newTitle); // 绘制文本输入框，用于输入新标题

            if (newTitle.Length >= 200)// 检查新标题的长度是否等于或超过200个字符
            {
                Messages.Message("标题的最大长度为200字符".Translate(), MessageTypeDefOf.RejectInput, false);// 准备一条消息，告知标题的最大长度限制，
                newTitle = newTitle.Substring(0, 200); // 如果标题超长，则截断文本，使其仅包含前200个字符
            }
            if (Widgets.ButtonText(new Rect(0f, 90f, inRect.width / 2f - 10f, 30f), "保存".Translate())) // 绘制保存按钮
            {
                if (newTitle.EndsWith(" ")) // 如果新标题以空格结尾
                {
                    Messages.Message("末尾不能带有空格".Translate(), MessageTypeDefOf.RejectInput, false); // 显示错误信息
                }
                else if (string.IsNullOrWhiteSpace(newTitle)) // 如果新标题为空或仅包含空白字符
                {
                    Messages.Message("标题不能为空".Translate(), MessageTypeDefOf.RejectInput, false); // 显示错误信息
                }
                else if (compArt.parent.GetComp<CompStyleable>()?.SourcePrecept != null) // 如果艺术品是文化类建筑
                {
                    Messages.Message("文化类建筑的标题不可修改".Translate(), MessageTypeDefOf.RejectInput, false); // 显示错误信息
                }
                else // 如果输入有效
                {
                    Traverse.Create(compArt).Field("titleInt").SetValue(new TaggedString(newTitle)); // 更新艺术品标题
                    Messages.Message("标题已修改".Translate(), MessageTypeDefOf.RejectInput, false); // 显示成功信息
                    Close(); // 关闭窗口
                }
            }

            if (Widgets.ButtonText(new Rect(inRect.width / 2f + 10f, 90f, inRect.width / 2f - 10f, 30f), "关闭".Translate())) // 绘制关闭按钮
            {
                Close(); // 关闭窗口
            }
        }
    }

    public class Dialog_EditArtAuthor : Window // 定义另一个窗口类，用于编辑艺术品作者
    {
        private readonly CompArt compArt; // 存储艺术组件的引用
        private string newAuthor; // 用于存储用户输入的新作者名

        public Dialog_EditArtAuthor(CompArt compArt) // 构造函数，用于初始化窗口
        {
            this.compArt = compArt; // 将传入的艺术组件赋值给类变量
            this.newAuthor = compArt.AuthorName; // 初始化输入框中的默认作者名为当前作者名
            this.doCloseButton = false; // 禁用窗口的底部关闭按钮
            this.doCloseX = true; // 启用窗口右上角的关闭按钮
            this.absorbInputAroundWindow = true; // 吸收窗口周围的用户输入，避免误操作
            this.forcePause = true; // 打开窗口时暂停游戏
        }

        public override Vector2 InitialSize => new Vector2(500f, 165f); // 设置窗口的初始尺寸

        public override void OnAcceptKeyPressed() => Event.current.Use(); // 禁止默认的确认按键行为，避免窗口误关闭

        public override void DoWindowContents(Rect inRect) // 重写方法，绘制窗口内容
        {
            Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "输入新的作者".Translate()); // 在窗口顶部显示作者输入提示
            newAuthor = Widgets.TextField(new Rect(0f, 40f, inRect.width, 30f), newAuthor); // 绘制文本输入框，用于输入新作者名

            if (newAuthor.Length >= 200)// 检查新作者的长度是否等于或超过200个字符
            {
                Messages.Message("作者的最大长度为200字符".Translate(), MessageTypeDefOf.RejectInput, false);// 准备一条消息，告知作者的最大长度限制
                newAuthor = newAuthor.Substring(0, 200); // 如果作者超长，则截断文本，使其仅包含前200个字符
            }
            if (Widgets.ButtonText(new Rect(0f, 90f, inRect.width / 2f - 10f, 30f), "保存".Translate())) // 绘制保存按钮
            {
                if (string.IsNullOrWhiteSpace(newAuthor)) // 如果新作者名为空或仅包含空白字符
                {
                    Messages.Message("作者不能为空".Translate(), MessageTypeDefOf.RejectInput, false); // 显示错误信息
                }
                else // 如果输入有效
                {
                    Traverse.Create(compArt).Field("authorNameInt").SetValue(new TaggedString(newAuthor)); // 更新艺术品作者
                    Messages.Message("作者已修改".Translate(), MessageTypeDefOf.RejectInput, false); // 显示成功信息
                    Close(); // 关闭窗口
                }
            }

            if (Widgets.ButtonText(new Rect(inRect.width / 2f + 10f, 90f, inRect.width / 2f - 10f, 30f), "关闭".Translate())) // 绘制关闭按钮
            {
                Close(); // 关闭窗口
            }
        }
    }

    public class Dialog_EditArtDescription : Window // 定义一个窗口类，用于编辑艺术品描述
    {
        private readonly CompArt compArt; // 存储艺术组件的引用
        private string newDescription; // 用于存储用户输入的新描述
        private static Vector2 scrollPosition; // 用于记录滚动视图的滚动位置

        public Dialog_EditArtDescription(CompArt compArt) // 构造函数，用于初始化窗口
        {
            this.compArt = compArt; // 将传入的艺术组件赋值给类变量
            this.newDescription = compArt.GenerateImageDescription().ToString(); // 初始化描述内容为当前描述
            doCloseButton = false; // 禁用窗口的底部关闭按钮
            doCloseX = true; // 启用窗口右上角的关闭按钮
            absorbInputAroundWindow = true; // 吸收窗口周围的用户输入，避免误操作
            forcePause = true; // 打开窗口时暂停游戏
        }

        public override void OnAcceptKeyPressed() => Event.current.Use(); // 禁止默认的确认按键行为，避免窗口误关闭

        public override void DoWindowContents(Rect inRect) // 重写方法，绘制窗口内容
        {
            Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "输入新的描述".Translate()); // 在窗口顶部显示描述输入提示

            Rect textAreaRect = new Rect(0f, 40f, inRect.width + 10f, inRect.height - 80f); // 定义文本区域的显示矩形
            float contentHeight = Text.CalcHeight(newDescription, textAreaRect.width - 16f) + 80f; // 计算描述内容所需的总高度
            Rect contentRect = new Rect(0f, 0f, inRect.width - 10f, Mathf.Max(contentHeight, textAreaRect.height)); // 定义滚动视图的内容矩形

            Widgets.BeginScrollView(textAreaRect, ref scrollPosition, contentRect); // 开始滚动视图
            newDescription = Widgets.TextArea(contentRect, newDescription); // 绘制文本输入区域
            if (newDescription.Length >= 30000)// 检查新描述的长度是否等于或超过30000个字符
            {
                Messages.Message("描述的最大长度为30000字符".Translate(), MessageTypeDefOf.RejectInput, false);// 准备一条消息，告知描述的最大长度限制
                newDescription.Substring(0, 30000);// 如果描述超长，则截断文本，使其仅包含前30000个字符
            }
            Widgets.EndScrollView(); // 结束滚动视图

            if (Widgets.ButtonText(new Rect(0f, inRect.height - 30f, inRect.width / 2f - 10f, 30f), "保存".Translate())) // 绘制保存按钮
            {
                if (string.IsNullOrWhiteSpace(newDescription)) // 如果新描述为空或仅包含空白字符
                {
                    Messages.Message("请勿空置描述".Translate(), MessageTypeDefOf.RejectInput, false); // 显示错误信息
                }
                else // 如果输入有效
                {
                    CompArt_GenerateImageDescription_Patch.SetDescription(compArt, newDescription); // 更新艺术品描述
                    Messages.Message("描述已修改".Translate(), MessageTypeDefOf.RejectInput, false); // 显示成功信息
                    Close(); // 关闭窗口
                }
            }

            if (Widgets.ButtonText(new Rect(inRect.width / 2f + 10f, inRect.height - 30f, inRect.width / 2f - 10f, 30f), "关闭".Translate())) // 绘制关闭按钮
            {
                Close(); // 关闭窗口
            }
        }
    }

    public class ArtEditorSettings : ModSettings // 定义一个类，用于存储和管理艺术编辑器的配置
    {
        public int InterfaceWidth = 400; // 界面的默认宽度
        public int InterfaceHeight = 300; // 界面的默认高度

        public override void ExposeData() // 重写方法，用于保存和加载配置
        {
            Scribe_Values.Look(ref InterfaceWidth, "InterfaceWidth", 400); // 保存/加载界面宽度，默认值为 400
            Scribe_Values.Look(ref InterfaceHeight, "InterfaceHeight", 300); // 保存/加载界面高度，默认值为 300
            base.ExposeData(); // 调用基类的方法
        }
    }

    public class ArtEditorMod : Mod // 定义一个类，用于初始化和管理艺术编辑器模组
    {
        private readonly ArtEditorSettings settings; // 存储模组的设置
        private string widthBuffer; // 缓存宽度输入框的内容
        private string heightBuffer; // 缓存高度输入框的内容

        public ArtEditorMod(ModContentPack content) // 构造函数，用于初始化模组
            : base(content)
        {
            settings = GetSettings<ArtEditorSettings>(); // 获取模组的设置对象
            widthBuffer = settings.InterfaceWidth.ToString(); // 初始化宽度输入框内容为当前宽度
            heightBuffer = settings.InterfaceHeight.ToString(); // 初始化高度输入框内容为当前高度
        }

        public override void DoSettingsWindowContents(Rect inRect) // 重写方法，绘制设置窗口内容
        {
            Listing_Standard listingStandard = new Listing_Standard(); // 创建一个标准列表，用于布局控件
            listingStandard.Begin(inRect); // 开始布局
            Text.Font = GameFont.Medium; // 设置字体为中等大小

            listingStandard.Label("编辑界面宽度".Translate()); // 显示宽度设置的标签
            listingStandard.TextFieldNumeric(ref settings.InterfaceWidth, ref widthBuffer); // 显示宽度输入框，允许输入数字

            listingStandard.Label("编辑界面高度".Translate()); // 显示高度设置的标签
            listingStandard.TextFieldNumeric(ref settings.InterfaceHeight, ref heightBuffer); // 显示高度输入框，允许输入数字

            listingStandard.Label("单位是像素".Translate()); // 显示像素单位的提示
            listingStandard.Label("宽度与高度的初始值分别是400与300".Translate()); // 显示默认宽度和高度的提示

            listingStandard.End(); // 结束布局
            base.DoSettingsWindowContents(inRect); // 调用基类的方法
        }

        public override string SettingsCategory() => "艺术编辑器".Translate(); // 返回设置类别的名称
    }

    [HarmonyPatch(typeof(CompArt), "PostExposeData")] // 为 CompArt 类的 PostExposeData 方法添加 Harmony 补丁
    public static class CompArt_PostExposeData_Patch
    {
        public static void Postfix(CompArt __instance) // 在 PostExposeData 方法执行后调用
        {
            string description = CompArt_GenerateImageDescription_Patch.GetDescription(__instance);// 获取当前艺术组件的描述
            Scribe_Values.Look(ref description, "DescriptionOverride", defaultValue: string.Empty);// 序列化描述
            CompArt_GenerateImageDescription_Patch.SetDescription(__instance, description);// 设置描述到组件
        }
    }

    [HarmonyPatch(typeof(CompArt), "GenerateImageDescription")] // 为 GenerateImageDescription 方法添加 Harmony 补丁
    public static class CompArt_GenerateImageDescription_Patch
    {
        private static readonly Dictionary<CompArt, string> descriptions = new Dictionary<CompArt, string>(); // 存储组件描述的字典

        public static void SetDescription(CompArt compArt, string description) => descriptions[compArt] = description; // 设置组件的描述

        public static string GetDescription(CompArt compArt) => descriptions.TryGetValue(compArt, out string desc) ? desc : null; // 获取组件的描述

        public static bool Prefix(CompArt __instance, ref TaggedString __result) // 在方法执行前调用
        {
            string description = GetDescription(__instance); // 获取组件的描述
            if (!string.IsNullOrEmpty(description)) // 如果描述不为空
            {
                __result = new TaggedString(description); // 设置方法的返回值
                return false; // 跳过原始方法的执行
            }
            return true; // 继续执行原始方法
        }

        public static void ClearAllDescriptions() // 清空所有描述
        {
            descriptions.Clear(); // 清空字典
        }
    }

    [HarmonyPatch(typeof(ThingComp), nameof(ThingComp.CompGetGizmosExtra))] // 为 ThingComp 类的 CompGetGizmosExtra 方法添加 Harmony 补丁
    public class CompGetGizmosExtraPatch
    {
        static void Postfix(ThingComp __instance, ref IEnumerable<Gizmo> __result) // 在方法执行后调用
        {
            if (__instance is CompArt compArt)// 如果组件是艺术组件
            {
                // 如果该艺术组件是石碑且未激活
                if (compArt.parent.ThingID.StartsWith("Stele") && !compArt.Active)
                {
                    // 激活艺术组件
                    compArt.InitializeArt(null);
                }

                if (compArt.CanShowArt && compArt.Active) // 如果艺术组件可显示艺术且激活
                {
                    __result = __result.Concat(new Gizmo[]
                    {
                    CreateGizmo("编辑标题", "编辑此艺术品的标题", () => Find.WindowStack.Add(new Dialog_EditArtTitle(compArt))),// 编辑标题
                    CreateGizmo("编辑作者", "编辑此艺术品的作者", () => Find.WindowStack.Add(new Dialog_EditArtAuthor(compArt))),// 编辑作者
                    CreateGizmo("编辑描述", "编辑此艺术品的描述", () => Find.WindowStack.Add(new Dialog_EditArtDescription(compArt)))// 编辑描述
                    });

                    if (DebugSettings.godMode) // 如果当前启用了上帝模式
                    {
                        __result = __result.Concat(new Gizmo[]  // 向 Gizmo 列表添加重置所有描述的选项
                        {
                        CreateGizmo("重置所有描述", "还原所有已修改的描述", () =>
                        {
                            CompArt_GenerateImageDescription_Patch.ClearAllDescriptions(); // 清空描述
                            Messages.Message("所有描述已重置".Translate(), MessageTypeDefOf.PositiveEvent); // 显示成功消息
                        })
                        });
                    }
                }
            }
        }

        private static Command_Action CreateGizmo(string label, string description, System.Action action) =>
            new Command_Action // 创建一个 Gizmo 选项
            {
                defaultLabel = label.Translate(), // 设置选项的标签
                defaultDesc = description.Translate(), // 设置选项的描述
                action = action, // 设置选项的执行操作
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/Rename") // 设置选项的图标
            };
    }

    [HarmonyPatch(typeof(ITab_Art), "FillTab")] // 为 ITab_Art 类的 FillTab 方法添加 Harmony 补丁
    public static class ITab_Art_FillTab_Patch
    {
        private static Vector2 scrollPosition; // 用于存储滚动视图的滚动位置
        private static Vector2 customSize; // 用于存储自定义的窗口大小

        public static void SetCustomSize() // 设置自定义窗口大小
        {
            var settings = LoadedModManager.GetMod<ArtEditorMod>().GetSettings<ArtEditorSettings>(); // 获取艺术编辑器的设置
            int width = settings.InterfaceWidth; // 获取设置的宽度
            int height = settings.InterfaceHeight; // 获取设置的高度
            customSize = new Vector2(width, height); // 设置自定义大小
        }

        public static bool Prefix(ITab_Art __instance) // 在方法执行前调用
        {
            CompArt compArt = Traverse.Create(__instance).Property("SelectedCompArt").GetValue<CompArt>(); // 获取当前选中的艺术组件
            if (compArt == null) // 如果没有选中组件
                return true; // 继续执行原始方法

            SetCustomSize(); // 设置自定义大小
            Traverse.Create(__instance).Field("size").SetValue(customSize); // 修改窗口大小

            Rect outRect = new Rect(0f, 0f, customSize.x, customSize.y).ContractedBy(10f); // 定义外部矩形
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, Text.CalcHeight(compArt.GenerateImageDescription(), outRect.width - 16f) + 80f); // 定义内容矩形

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect); // 开始滚动视图
            Text.Font = GameFont.Medium; // 设置字体为中等大小
            Widgets.Label(new Rect(0f, 0f, viewRect.width, 30f), compArt.Title.Truncate(viewRect.width)); // 显示标题
            Text.Font = GameFont.Small; // 设置字体为小
            Widgets.Label(new Rect(0f, 35f, viewRect.width, viewRect.height - 35f), compArt.GenerateImageDescription()); // 显示描述
            Widgets.EndScrollView(); // 结束滚动视图

            return false; // 跳过原始方法的执行
        }
    }
}
