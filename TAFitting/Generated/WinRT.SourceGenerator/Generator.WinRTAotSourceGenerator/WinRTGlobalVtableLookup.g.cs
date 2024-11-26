using System; 
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace WinRT.TAFittingGenericHelpers
{

    internal static class GlobalVtableLookup
    {

        [System.Runtime.CompilerServices.ModuleInitializer]
        internal static void InitializeGlobalVtableLookup()
        {
            ComWrappersSupport.RegisterTypeComInterfaceEntriesLookup(new Func<Type, ComWrappers.ComInterfaceEntry[]>(LookupVtableEntries));
            ComWrappersSupport.RegisterTypeRuntimeClassNameLookup(new Func<Type, string>(LookupRuntimeClassName));
        }

        private static ComWrappers.ComInterfaceEntry[] LookupVtableEntries(Type type)
        {
            string typeName = type.ToString();
            if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Collections.Generic.IEnumerable`1[System.Object]]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Collections_Generic_IEnumerable_object_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Collections_IEnumerable.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.IEnumerable<object>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.IEnumerable<object>>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.IEnumerable>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.IEnumerable>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "System.Collections.ObjectModel.ReadOnlyDictionary`2[System.Double,System.Double[]]"
            || typeName == "System.Collections.Generic.Dictionary`2[System.Guid,System.Collections.Generic.List`1[TAFitting.Model.IEstimateProvider]]"
            || typeName == "ABI.System.Collections.Generic.ConstantSplittableMap`2[System.Guid,System.Collections.Generic.List`1[TAFitting.Model.IEstimateProvider]]"
            || typeName == "ABI.System.Collections.Generic.ConstantSplittableMap`2[System.Guid,TAFitting.Model.ModelItem]"
            || typeName == "ABI.System.Collections.Generic.ConstantSplittableMap`2[System.Double,System.Double[]]"
            || typeName == "System.Collections.ObjectModel.ReadOnlyDictionary`2[System.Guid,System.Collections.Generic.List`1[TAFitting.Model.IEstimateProvider]]"
            || typeName == "System.Collections.ObjectModel.ReadOnlyDictionary`2[System.Guid,TAFitting.Model.ModelItem]"
            || typeName == "System.Collections.Generic.Dictionary`2[System.Guid,TAFitting.Model.ModelItem]"
            || typeName == "System.Collections.Generic.Dictionary`2[System.Double,System.Double[]]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
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
            if (typeName == "System.Collections.Generic.Dictionary`2+ValueCollection[System.Double,TAFitting.Data.Decay]"
            )
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
            if (typeName == "System.Collections.Generic.KeyValuePair`2[System.String,System.String]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.KeyValuePair_string_string.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.KeyValuePairMethods<string, string>.IID,
                Vtable = global::ABI.System.Collections.Generic.KeyValuePairMethods<string, string>.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Object]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "System.Collections.ObjectModel.ReadOnlyCollection`1[TAFitting.Model.Parameter]"
            || typeName == "System.Collections.Generic.List`1[TAFitting.Model.Parameter]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IListMethods.IID,
                Vtable = global::ABI.System.Collections.IListMethods.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IEnumerableMethods.IID,
                Vtable = global::ABI.System.Collections.IEnumerableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "ABI.System.Collections.Generic.ConstantSplittableMap`2[System.String,System.String]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyDictionary_string_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.KeyValuePair_string_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_System_Collections_Generic_KeyValuePair_string__string_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyDictionaryMethods<string, string>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyDictionaryMethods<string, string>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.Generic.KeyValuePair<string, string>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.Generic.KeyValuePair<string, string>>.AbiToProjectionVftablePtr
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
            if (typeName == "System.Collections.Generic.Dictionary`2+KeyCollection[System.Double,TAFitting.Data.Decay]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_double.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<double>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<double>.AbiToProjectionVftablePtr
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
            if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Double]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_double.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<double>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<double>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "System.String[]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IList_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_char.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_System_Collections_Generic_IEnumerable_char_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_System_Collections_Generic_IEnumerable_object_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_System_Collections_IEnumerable.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_System_Collections_Generic_IEnumerable_char_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_System_Collections_Generic_IEnumerable_object_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_System_Collections_IEnumerable.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IListMethods.IID,
                Vtable = global::ABI.System.Collections.IListMethods.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IListMethods<string>.IID,
                Vtable = global::ABI.System.Collections.Generic.IListMethods<string>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<string>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<string>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<global::System.Collections.Generic.IEnumerable<char>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<global::System.Collections.Generic.IEnumerable<char>>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<global::System.Collections.Generic.IEnumerable<object>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<global::System.Collections.Generic.IEnumerable<object>>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<global::System.Collections.IEnumerable>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<global::System.Collections.IEnumerable>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<string>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<string>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.Generic.IEnumerable<char>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.Generic.IEnumerable<char>>.AbiToProjectionVftablePtr
            },
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
            if (typeName == "System.Windows.Forms.SplitterPanel"
            || typeName == "System.Windows.Forms.Label"
            )
            {
                
        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "System.Double[]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IList_double.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_double.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_double.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IListMethods.IID,
                Vtable = global::ABI.System.Collections.IListMethods.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IListMethods<double>.IID,
                Vtable = global::ABI.System.Collections.Generic.IListMethods<double>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<double>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<double>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<double>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<double>.AbiToProjectionVftablePtr
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
            if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Collections.Generic.KeyValuePair`2[System.String,System.String]]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.KeyValuePair_string_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Collections_Generic_KeyValuePair_string__string_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.KeyValuePair<string, string>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.KeyValuePair<string, string>>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "System.Collections.Generic.List`1[System.Double]"
            || typeName == "System.Collections.ObjectModel.ReadOnlyCollection`1[System.Double]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IList_double.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_double.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_double.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IListMethods<double>.IID,
                Vtable = global::ABI.System.Collections.Generic.IListMethods<double>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<double>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<double>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<double>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<double>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IListMethods.IID,
                Vtable = global::ABI.System.Collections.IListMethods.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IEnumerableMethods.IID,
                Vtable = global::ABI.System.Collections.IEnumerableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "System.Guid[]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IList_System_Guid.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_System_Guid.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_System_Guid.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IListMethods.IID,
                Vtable = global::ABI.System.Collections.IListMethods.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IListMethods<global::System.Guid>.IID,
                Vtable = global::ABI.System.Collections.Generic.IListMethods<global::System.Guid>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<global::System.Guid>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<global::System.Guid>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Guid>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Guid>.AbiToProjectionVftablePtr
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
            if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Guid]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Guid.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Guid>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Guid>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Collections.IEnumerable]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Collections_IEnumerable.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.IEnumerable>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.IEnumerable>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Int32]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_int.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<int>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<int>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Collections.Generic.IEnumerable`1[System.Char]]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_char.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Collections_Generic_IEnumerable_char_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Collections_IEnumerable.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.IEnumerable<char>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.IEnumerable<char>>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.IEnumerable>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.IEnumerable>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.String]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_char.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Collections_Generic_IEnumerable_char_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Collections_Generic_IEnumerable_object_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_System_Collections_IEnumerable.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerator_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<string>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<string>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.IEnumerable<char>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.IEnumerable<char>>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.IEnumerable<object>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.Generic.IEnumerable<object>>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.IEnumerable>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<global::System.Collections.IEnumerable>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumeratorMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.IDisposableMethods.IID,
                Vtable = global::ABI.System.IDisposableMethods.AbiToProjectionVftablePtr
            },
};

            }
            if (typeName == "System.Int32[]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IList_int.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_int.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_int.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IListMethods.IID,
                Vtable = global::ABI.System.Collections.IListMethods.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IListMethods<int>.IID,
                Vtable = global::ABI.System.Collections.Generic.IListMethods<int>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<int>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<int>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<int>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<int>.AbiToProjectionVftablePtr
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
            if (typeName == "System.Collections.ObjectModel.ReadOnlyDictionary`2[System.String,System.String]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IDictionary_string_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyDictionary_string_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.KeyValuePair_string_string.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_System_Collections_Generic_KeyValuePair_string__string_.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IDictionaryMethods<string, string>.IID,
                Vtable = global::ABI.System.Collections.Generic.IDictionaryMethods<string, string>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyDictionaryMethods<string, string>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyDictionaryMethods<string, string>.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.Generic.KeyValuePair<string, string>>.IID,
                Vtable = global::ABI.System.Collections.Generic.IEnumerableMethods<global::System.Collections.Generic.KeyValuePair<string, string>>.AbiToProjectionVftablePtr
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
            if (typeName == "TAFitting.Model.Parameter[]"
            )
            {
                        _ = global::WinRT.TAFittingGenericHelpers.IReadOnlyList_object.Initialized;
        _ = global::WinRT.TAFittingGenericHelpers.IEnumerable_object.Initialized;

        return new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry[]
        {
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.IListMethods.IID,
                Vtable = global::ABI.System.Collections.IListMethods.AbiToProjectionVftablePtr
            },
            new global::System.Runtime.InteropServices.ComWrappers.ComInterfaceEntry
            {
                IID = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.IID,
                Vtable = global::ABI.System.Collections.Generic.IReadOnlyListMethods<object>.AbiToProjectionVftablePtr
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
            return default;
        }
private static string LookupRuntimeClassName(Type type)
{
    string typeName = type.ToString();
if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Collections.Generic.IEnumerable`1[System.Object]]"
)
{
    return "Windows.Foundation.Collections.IIterator`1<Windows.Foundation.Collections.IIterable`1<Object>>";
}
if (typeName == "System.Collections.ObjectModel.ReadOnlyDictionary`2[System.Double,System.Double[]]"
|| typeName == "System.Collections.Generic.Dictionary`2[System.Guid,System.Collections.Generic.List`1[TAFitting.Model.IEstimateProvider]]"
|| typeName == "ABI.System.Collections.Generic.ConstantSplittableMap`2[System.Guid,System.Collections.Generic.List`1[TAFitting.Model.IEstimateProvider]]"
|| typeName == "ABI.System.Collections.Generic.ConstantSplittableMap`2[System.Guid,TAFitting.Model.ModelItem]"
|| typeName == "ABI.System.Collections.Generic.ConstantSplittableMap`2[System.Double,System.Double[]]"
|| typeName == "System.Collections.ObjectModel.ReadOnlyDictionary`2[System.Guid,System.Collections.Generic.List`1[TAFitting.Model.IEstimateProvider]]"
|| typeName == "System.Collections.ObjectModel.ReadOnlyDictionary`2[System.Guid,TAFitting.Model.ModelItem]"
|| typeName == "System.Collections.Generic.Dictionary`2[System.Guid,TAFitting.Model.ModelItem]"
|| typeName == "System.Collections.Generic.Dictionary`2[System.Double,System.Double[]]"
)
{
    return "Windows.Foundation.Collections.IIterable`1<Object>";
}
if (typeName == "System.Collections.Generic.Dictionary`2+ValueCollection[System.Double,TAFitting.Data.Decay]"
)
{
    return "Windows.Foundation.Collections.IIterable`1<Windows.Foundation.Collections.IIterable`1<Object>>";
}
if (typeName == "System.Collections.Generic.KeyValuePair`2[System.String,System.String]"
)
{
    return "Windows.Foundation.Collections.IKeyValuePair`2<String, String>";
}
if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Object]"
)
{
    return "Windows.Foundation.Collections.IIterator`1<Object>";
}
if (typeName == "System.Collections.ObjectModel.ReadOnlyCollection`1[TAFitting.Model.Parameter]"
|| typeName == "System.Collections.Generic.List`1[TAFitting.Model.Parameter]"
)
{
    return "Windows.Foundation.Collections.IVectorView`1<Object>";
}
if (typeName == "ABI.System.Collections.Generic.ConstantSplittableMap`2[System.String,System.String]"
)
{
    return "Windows.Foundation.Collections.IMapView`2<String, String>";
}
if (typeName == "System.Collections.Generic.Dictionary`2+KeyCollection[System.Double,TAFitting.Data.Decay]"
)
{
    return "Windows.Foundation.Collections.IIterable`1<Double>";
}
if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Double]"
)
{
    return "Windows.Foundation.Collections.IIterator`1<Double>";
}
if (typeName == "System.String[]"
|| typeName == "System.Double[]"
|| typeName == "System.Guid[]"
|| typeName == "System.Int32[]"
|| typeName == "TAFitting.Model.Parameter[]"
)
{
    return "Microsoft.UI.Xaml.Interop.IBindableVector";
}
if (typeName == "System.Windows.Forms.SplitterPanel"
|| typeName == "System.Windows.Forms.Label"
)
{
    return "Windows.Foundation.IClosable";
}
if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Collections.Generic.KeyValuePair`2[System.String,System.String]]"
)
{
    return "Windows.Foundation.Collections.IIterator`1<Windows.Foundation.Collections.IKeyValuePair`2<String, String>>";
}
if (typeName == "System.Collections.Generic.List`1[System.Double]"
|| typeName == "System.Collections.ObjectModel.ReadOnlyCollection`1[System.Double]"
)
{
    return "Windows.Foundation.Collections.IVector`1<Double>";
}
if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Guid]"
)
{
    return "Windows.Foundation.Collections.IIterator`1<System.Guid>";
}
if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Collections.IEnumerable]"
)
{
    return "Windows.Foundation.Collections.IIterator`1<Microsoft.UI.Xaml.Interop.IBindableIterable>";
}
if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Int32]"
)
{
    return "Windows.Foundation.Collections.IIterator`1<Int32>";
}
if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.Collections.Generic.IEnumerable`1[System.Char]]"
)
{
    return "Windows.Foundation.Collections.IIterator`1<Windows.Foundation.Collections.IIterable`1<Char>>";
}
if (typeName == "ABI.System.Collections.Generic.ToAbiEnumeratorAdapter`1[System.String]"
)
{
    return "Windows.Foundation.Collections.IIterator`1<String>";
}
if (typeName == "System.Collections.ObjectModel.ReadOnlyDictionary`2[System.String,System.String]"
)
{
    return "Windows.Foundation.Collections.IMap`2<String, String>";
}
            return default;
        }
    }
}
