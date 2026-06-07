# بناء ملف APK

هذا الملف يشرح خطوات بناء تطبيق **AR Education** كملف APK لنظام Android.

## إعدادات Android الحالية

الإعدادات الموجودة في المشروع:

| الإعداد | القيمة |
|---|---|
| اسم المنتج | `AR Education` |
| Bundle Identifier | `com.areducation.app` |
| إصدار التطبيق | `1.0` |
| أقل Android API | `24` |
| Android Target SDK | `34` |
| المعمارية | ARM64 |
| AR Plugin | ARCore |
| ملف Android Manifest | `Assets/Plugins/Android/AndroidManifest.xml` |

يتضمن `AndroidManifest.xml` الصلاحيات والخصائص المهمة التالية:

```text
CAMERA

android.hardware.camera.ar
com.google.ar.core = required
```

## تثبيت وحدة Android في Unity

من Unity Hub:

1. افتح `Installs`.
2. اختر نسخة Unity المستخدمة في المشروع.
3. اضغط على قائمة النقاط بجانبها.
4. اختر `Add modules`.
5. فعّل:

```text
Android Build Support
Android SDK & NDK Tools
OpenJDK
```

6. اضغط `Install`.

## بناء APK من واجهة Unity

1. افتح المشروع في Unity.
2. شغّل تجهيز المشاهد:

```text
AR Education > Setup All Scenes
```

3. افتح:

```text
File > Build Settings
```

4. اختر `Android`.
5. اضغط `Switch Platform`.
6. تأكد أن المشاهد التالية موجودة ومفعلة في القائمة وبنفس الترتيب:

```text
0: Assets/Scenes/MainMenu.unity
1: Assets/Scenes/ARLesson.unity
2: Assets/Scenes/Quiz.unity
3: Assets/Scenes/TeacherDashboard.unity
```

هذه المشاهد موجودة أيضًا في `ProjectSettings/EditorBuildSettings.asset`.

7. اضغط `Player Settings` وتحقق من:

```text
Player > Other Settings > Identification > Package Name = com.areducation.app
Player > Other Settings > Minimum API Level = Android 7.0 API 24
Player > Other Settings > Target API Level = API 34 أو Automatic
Player > Other Settings > Target Architectures = ARM64
```

8. من `Project Settings > XR Plug-in Management` تأكد أن `ARCore` مفعّل في تبويب Android.
9. عد إلى نافذة Build Settings.
10. اضغط `Build`.
11. اختر مجلدًا للحفظ، مثل:

```text
Builds/Android/AR-Education.apk
```

12. انتظر حتى ينتهي Unity من عملية البناء.

## البناء والتثبيت مباشرة على الهاتف

إذا كان الهاتف متصلًا و`USB Debugging` مفعّلًا:

1. افتح `File > Build Settings`.
2. اختر `Android`.
3. اضغط:

```text
Build And Run
```

سيبني Unity ملف APK ثم يثبته ويشغله على الهاتف.

## بناء APK من سطر الأوامر

يمكن تشغيل Unity بوضع Batch لبناء APK. غيّر مسار Unity حسب مكان تثبيته عندك.

على macOS مثال:

```bash
/Applications/Unity/Hub/Editor/6000.4.8f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath /Users/halim/AR-project-main \
  -buildTarget Android \
  -buildAndroidPlayer /Users/halim/AR-project-main/Builds/Android/AR-Education.apk
```

ملاحظات:

- سكربت `Assets/Editor/BuildPreprocessor.cs` يشغّل تجهيز المشاهد تلقائيًا قبل البناء.
- إذا فشل البناء بسبب Android SDK أو NDK، تأكد أن Unity يستخدم الأدوات المثبتة مع Unity Hub من `Preferences > External Tools`.
- إذا احتجت توقيعًا للإصدار النهائي، أضف Keystore من إعدادات `Player Settings > Publishing Settings`.

## إنشاء نسخة Release موقعة

لإنشاء APK مناسب للنشر:

1. افتح:

```text
Edit > Project Settings > Player > Android > Publishing Settings
```

2. فعّل `Custom Keystore`.
3. اختر ملف Keystore أو أنشئ واحدًا جديدًا.
4. أدخل كلمات المرور واسم المفتاح.
5. تأكد من تعطيل `Development Build` في Build Settings.
6. اضغط `Build`.

احتفظ بملف Keystore وكلمات المرور في مكان آمن. لا يمكن تحديث التطبيق على المتجر بنفس الحزمة إذا فقدت مفتاح التوقيع.

## التحقق بعد بناء APK

بعد الحصول على APK:

1. ثبّت الملف على جهاز Android يدعم ARCore.
2. افتح التطبيق.
3. تحقق من ظهور القائمة الرئيسية.
4. افتح درس AR وتأكد أن التطبيق يطلب صلاحية الكاميرا.
5. وجّه الكاميرا إلى سطح مستو وانتظر اكتشاف السطح.
6. ضع نموذج الدرس وتأكد من إمكانية التفاعل معه.
7. افتح اختبارًا واحفظ نتيجة.
8. افتح لوحة المعلم وتأكد أن النتيجة تظهر.

## أخطاء بناء شائعة

### No valid Android SDK found

الحل:

- ثبّت `Android Build Support` و`Android SDK & NDK Tools` من Unity Hub.
- افتح `Unity > Settings/Preferences > External Tools`.
- فعّل استخدام الأدوات المرفقة مع Unity.

### ARCore غير مفعّل

الحل:

```text
Edit > Project Settings > XR Plug-in Management > Android > ARCore
```

### فشل IL2CPP أو مشاكل Stripping

المشروع يحتوي على:

```text
Assets/link.xml
```

إذا أضفت نماذج بيانات جديدة يتم تحويلها إلى JSON وتظهر مشاكل في نسخة IL2CPP، أضف الأنواع الجديدة إلى `link.xml`.

### الهاتف لا يفتح درس AR

تحقق من:

- الهاتف يدعم ARCore.
- صلاحية الكاميرا ممنوحة.
- خدمة Google Play Services for AR مثبتة.
- ملف APK مبني للمعمارية ARM64.
