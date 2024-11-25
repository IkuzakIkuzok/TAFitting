namespace WinRT.TAFittingVtableClasses
{
internal sealed class TAFitting_Data_DecaysWinRTTypeDetails : global::WinRT.IWinRTExposedTypeDetails
{
    public global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[] GetExposedInterfaces()
    {
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_System_Collections_Generic_IEnumerable_object_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_System_Collections_IEnumerable.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.Generic.IEnumerable<object>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.Generic.IEnumerable<object>>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.IEnumerable>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.IEnumerable>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IEnumerableMethods.IID,
                Vtable = global::ABI.System.Collections.IEnumerableMethods.AbiToProjectionVftablePtr
            },
};
        }
    }
}
