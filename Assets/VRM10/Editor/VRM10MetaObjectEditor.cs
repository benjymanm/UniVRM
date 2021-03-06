using UnityEditor;
using UnityEngine;
using MeshUtility.M17N;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10MetaObject))]
    public class VRMM10etaObjectEditor : Editor
    {
        class ValidateProperty
        {
            public SerializedProperty m_prop;

            public delegate (string, MessageType) Validator(SerializedProperty prop);
            Validator m_validator;

            public ValidateProperty(SerializedProperty prop, Validator validator)
            {
                m_prop = prop;
                m_validator = validator;
            }

            public void OnGUI()
            {
                // var old = m_prop.stringValue;
                if (m_prop.propertyType == SerializedPropertyType.Generic)
                {
                    if (m_prop.arrayElementType != null)
                    {
                        EditorGUILayout.LabelField(m_prop.name);

                        var depth = m_prop.depth;
                        var iterator = m_prop.Copy();
                        for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
                        {
                            if (iterator.depth < depth)
                                break;

                            depth = iterator.depth;

                            // using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                                EditorGUILayout.PropertyField(iterator, true);
                        }
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(m_prop);
                }
                var (msg, msgType) = m_validator(m_prop);
                if (!string.IsNullOrEmpty(msg))
                {
                    EditorGUILayout.HelpBox(msg, msgType);
                }
                // return old != m_prop.stringValue;
            }
        }

        VRM10MetaObject m_target;
        SerializedProperty m_Script;
        SerializedProperty m_exporterVersion;
        SerializedProperty m_thumbnail;
        ValidateProperty m_title;
        ValidateProperty m_version;
        ValidateProperty m_author;
        ValidateProperty m_contact;
        ValidateProperty m_reference;

        SerializedProperty m_AllowedUser;
        SerializedProperty m_ViolentUssage;
        SerializedProperty m_SexualUssage;
        SerializedProperty m_CommercialUssage;
        SerializedProperty m_PoliticalOrReligiousUsage;
        SerializedProperty m_OtherPermissionUrl;

        SerializedProperty m_LicenseType;
        SerializedProperty m_OtherLicenseUrl;

        static string RequiredMessage(string name)
        {
            switch (MeshUtility.M17N.Getter.Lang)
            {
                case MeshUtility.M17N.Languages.ja:
                    return $"必須項目。{name} を入力してください";

                case MeshUtility.M17N.Languages.en:
                    return $"{name} is required";

                default:
                    throw new System.NotImplementedException();
            }
        }

        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            m_target = (VRM10MetaObject)target;

            m_Script = serializedObject.FindProperty("m_Script");
            m_exporterVersion = serializedObject.FindProperty(nameof(m_target.ExporterVersion));
            m_thumbnail = serializedObject.FindProperty(nameof(m_target.Thumbnail));

            m_title = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.Name)), prop =>
                        {
                            if (string.IsNullOrEmpty(prop.stringValue))
                            {
                                return (RequiredMessage(prop.name), MessageType.Error);
                            }
                            return ("", MessageType.None);
                        });
            m_version = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.Version)), prop =>
                        {
                            // if (string.IsNullOrEmpty(prop.stringValue))
                            // {
                            //     return (RequiredMessage(prop.name), MessageType.Error);
                            // }
                            return ("", MessageType.None);
                        });
            m_author = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.Authors)), prop =>
                        {
                            if (prop.arraySize == 0)
                            {
                                return (RequiredMessage(prop.name), MessageType.Error);
                            }
                            return ("", MessageType.None);
                        });
            m_contact = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.ContactInformation)), prop =>
                        {
                            return ("", MessageType.None);
                        });
            m_reference = new ValidateProperty(serializedObject.FindProperty(nameof(m_target.Reference)), prop =>
                        {
                            return ("", MessageType.None);
                        });

            m_AllowedUser = serializedObject.FindProperty(nameof(m_target.AllowedUser));
            m_ViolentUssage = serializedObject.FindProperty(nameof(m_target.ViolentUsage));
            m_SexualUssage = serializedObject.FindProperty(nameof(m_target.SexualUsage));
            m_CommercialUssage = serializedObject.FindProperty(nameof(m_target.CommercialUsage));
            m_PoliticalOrReligiousUsage = serializedObject.FindProperty(nameof(m_target.PoliticalOrReligiousUsage));
            m_OtherPermissionUrl = serializedObject.FindProperty(nameof(m_target.OtherLicenseUrl));

            // m_LicenseType = serializedObject.FindProperty(nameof(m_target.));

            m_OtherLicenseUrl = serializedObject.FindProperty(nameof(m_target.OtherLicenseUrl));
        }

        enum MessageKeys
        {
            [LangMsg(Languages.ja, "アバターの人格に関する許諾範囲")]
            [LangMsg(Languages.en, "Personation / Characterization Permission")]
            PERSONATION,

            [LangMsg(Languages.ja, "アバターに人格を与えることの許諾範囲")]
            [LangMsg(Languages.en, "A person who can perform with this avatar")]
            ALLOWED_USER,

            [LangMsg(Languages.ja, "このアバターを用いて暴力表現を演じることの許可")]
            [LangMsg(Languages.en, "Violent acts using this avatar")]
            VIOLENT_USAGE,

            [LangMsg(Languages.ja, "このアバターを用いて性的表現を演じることの許可")]
            [LangMsg(Languages.en, "Sexuality acts using this avatar")]
            SEXUAL_USAGE,

            [LangMsg(Languages.ja, "商用利用の許可")]
            [LangMsg(Languages.en, "For commercial use")]
            COMMERCIAL_USAGE,

            [LangMsg(Languages.ja, "再配布・改変に関する許諾範囲")]
            [LangMsg(Languages.en, "Redistribution / Modifications License")]
            REDISTRIBUTION_MODIFICATIONS,

            // [LangMsg(Languages.ja, "")]
            // [LangMsg(Languages.en, "")]
        }

        static string Msg(MessageKeys key)
        {
            return MeshUtility.M17N.Getter.Msg(key);
        }

        bool m_foldoutInfo = true;
        bool m_foldoutPermission = true;
        bool m_foldoutDistribution = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (VRMVersion.IsNewer(m_exporterVersion.stringValue))
            {
                EditorGUILayout.HelpBox("Check UniVRM new version. https://github.com/dwango/UniVRM/releases", MessageType.Warning);
            }

            // texture
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_exporterVersion);
                GUI.enabled = true;
                EditorGUILayout.PropertyField(m_thumbnail);
                EditorGUILayout.EndVertical();
                m_thumbnail.objectReferenceValue = TextureField("", (Texture2D)m_thumbnail.objectReferenceValue, 100);
            }
            EditorGUILayout.EndHorizontal();

            m_foldoutInfo = EditorGUILayout.Foldout(m_foldoutInfo, "Information");
            if (m_foldoutInfo)
            {
                m_title.OnGUI();
                m_version.OnGUI();
                m_author.OnGUI();
                m_contact.OnGUI();
                m_reference.OnGUI();
            }
            // EditorGUILayout.LabelField("License ", EditorStyles.boldLabel);
            m_foldoutPermission = EditorGUILayout.Foldout(m_foldoutPermission, Msg(MessageKeys.PERSONATION));
            if (m_foldoutPermission)
            {
                var backup = EditorGUIUtility.labelWidth;
                RightFixedPropField(m_AllowedUser, Msg(MessageKeys.ALLOWED_USER));
                RightFixedPropField(m_ViolentUssage, Msg(MessageKeys.VIOLENT_USAGE));
                RightFixedPropField(m_SexualUssage, Msg(MessageKeys.SEXUAL_USAGE));
                RightFixedPropField(m_CommercialUssage, Msg(MessageKeys.COMMERCIAL_USAGE));
                EditorGUILayout.PropertyField(m_OtherPermissionUrl, new GUIContent("Other License Url"));
                EditorGUIUtility.labelWidth = backup;
            }

            m_foldoutDistribution = EditorGUILayout.Foldout(m_foldoutDistribution, Msg(MessageKeys.REDISTRIBUTION_MODIFICATIONS));
            if (m_foldoutDistribution)
            {
                // var licenseType = m_LicenseType;
                // EditorGUILayout.PropertyField(licenseType);
                // if ((LicenseType)licenseType.intValue == LicenseType.Other)
                // {
                //     EditorGUILayout.PropertyField(m_OtherLicenseUrl);
                // }
            }

            serializedObject.ApplyModifiedProperties();
        }

        static (Rect, Rect) FixedRight(Rect r, int width)
        {
            if (width > r.width)
            {
                width = (int)r.width;
            }
            return (
                new Rect(r.x, r.y, r.width - width, r.height),
                new Rect(r.x + r.width - width, r.y, width, r.height)
            );
        }

        static void RightFixedPropField(SerializedProperty prop, string label)
        {
            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            var (left, right) = FixedRight(r, 64);
            // Debug.Log($"{left}, {right}");
            EditorGUI.LabelField(left, label);
            EditorGUI.PropertyField(right, prop, new GUIContent(""), false);
        }

        private static Texture2D TextureField(string name, Texture2D texture, int size)
        {
            GUILayout.BeginHorizontal();
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            //style.fixedWidth = size;
            GUILayout.Label(name, style);
            var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(size), GUILayout.Height(size));
            GUILayout.EndVertical();
            return result;
        }
    }
}
