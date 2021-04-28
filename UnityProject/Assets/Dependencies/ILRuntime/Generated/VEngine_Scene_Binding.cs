using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class VEngine_Scene_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(VEngine.Scene);
            args = new Type[]{typeof(System.String), typeof(System.Action<VEngine.Scene>), typeof(System.Boolean)};
            method = type.GetMethod("LoadAsync", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LoadAsync_0);

            field = type.GetField("completed", flag);
            app.RegisterCLRFieldGetter(field, get_completed_0);
            app.RegisterCLRFieldSetter(field, set_completed_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_completed_0, AssignFromStack_completed_0);


        }


        static StackObject* LoadAsync_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @additive = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Action<VEngine.Scene> @completed = (System.Action<VEngine.Scene>)typeof(System.Action<VEngine.Scene>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.String @assetPath = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = VEngine.Scene.LoadAsync(@assetPath, @completed, @additive);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_completed_0(ref object o)
        {
            return ((VEngine.Scene)o).completed;
        }

        static StackObject* CopyToStack_completed_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((VEngine.Scene)o).completed;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_completed_0(ref object o, object v)
        {
            ((VEngine.Scene)o).completed = (System.Action<VEngine.Scene>)v;
        }

        static StackObject* AssignFromStack_completed_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<VEngine.Scene> @completed = (System.Action<VEngine.Scene>)typeof(System.Action<VEngine.Scene>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((VEngine.Scene)o).completed = @completed;
            return ptr_of_this_method;
        }



    }
}
