﻿//MIT, 2017, WinterDev


namespace YourImplementation
{

#if GL_ENABLE
    public static class BootStrapOpenGLES2
    {

        static bool s_initInit;
        public static void SetupDefaultValues()
        {
            if (s_initInit) return;
            //----
            s_initInit = true;
            //
            OpenTK.Toolkit.Init();
            //use common font loader
            //user can create and use other font-loader
            CommonTextServiceSetup.SetupDefaultValues();
            PixelFarm.Drawing.GLES2.GLES2Platform.SetFontLoader(CommonTextServiceSetup.myFontLoader);
        }
    }
#endif

}