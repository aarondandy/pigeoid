using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Pigeoid.Epsg.Resources;
using System.Collections.ObjectModel;

namespace Pigeoid.Epsg
{
    
    [Obsolete]
    public class EpsgCoordinateOperationInfoRepository
    {

        [Obsolete]
        internal static readonly EpsgDataResourceReaderCoordinateConversionInfo ReaderConversion = new EpsgDataResourceReaderCoordinateConversionInfo();

        [Obsolete]
        internal static readonly EpsgDataResourceReaderCoordinateTransformInfo ReaderTransform = new EpsgDataResourceReaderCoordinateTransformInfo();

        [Obsolete]
        internal static readonly EpsgDataResourceReaderConcatenatedCoordinateOperationInfo ReaderConcatenated = new EpsgDataResourceReaderConcatenatedCoordinateOperationInfo();


        [Obsolete]
        private static readonly ReadOnlyCollection<EpsgCoordinateTransformInfo> EmptyEcti =
            Array.AsReadOnly(new EpsgCoordinateTransformInfo[0]);

        [Obsolete]
        private static readonly ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo> EmptyEccoi =
            Array.AsReadOnly(new EpsgConcatenatedCoordinateOperationInfo[0]);

        [Obsolete]
        public static EpsgCoordinateTransformInfo GetTransformInfo(int code) {
            return code > 0 && code < UInt16.MaxValue ? ReaderTransform.GetByKey((ushort)code) : null;
        }

        [Obsolete]
        public static EpsgCoordinateOperationInfo GetConversionInfo(int code) {
            return code > 0 && code < UInt16.MaxValue ? ReaderConversion.GetByKey((ushort)code) : null;
        }

        [Obsolete]
        public static EpsgConcatenatedCoordinateOperationInfo GetConcatenatedInfo(int code) {
            return code > 0 && code < UInt16.MaxValue ? ReaderConcatenated.GetByKey((ushort)code) : null;
        }

        [Obsolete]
        /// <summary>
        /// Finds either a transformation or conversion for the given code.
        /// </summary>
        /// <param name="code">The code to find.</param>
        /// <returns>The operation for the code.</returns>
        internal static EpsgCoordinateOperationInfo GetSingleOperationInfo(int code) {
            if (code <= 0 || code >= UInt16.MaxValue)
                return null;
            var codeShort = unchecked((ushort)code);
            return ReaderTransform.GetByKey(codeShort)
                ?? ReaderConversion.GetByKey(codeShort);
        }

        [Obsolete]
        public static IEnumerable<EpsgCoordinateTransformInfo> TransformInfos {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateTransformInfo>>() != null);
                return ReaderTransform.ReadAllValues();
            }
        }

        [Obsolete]
        public static IEnumerable<EpsgCoordinateOperationInfo> ConversionInfos {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateOperationInfo>>() != null);
                return ReaderConversion.ReadAllValues();
            }
        }

        [Obsolete]
        public static IEnumerable<EpsgConcatenatedCoordinateOperationInfo> ConcatenatedInfos {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgConcatenatedCoordinateOperationInfo>>() != null);
                return ReaderConcatenated.ReadAllValues();
            }
        }

        [Obsolete]
        public static ReadOnlyCollection<EpsgCoordinateTransformInfo> GetTransformForwardReferenced(int sourceCode) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgCoordinateTransformInfo>>() != null);
            throw new NotImplementedException();
            //return ConvertToOperations(TransformLookUp.GetForwardReferencedOperationCodes(sourceCode));
        }

        [Obsolete]
        public static ReadOnlyCollection<EpsgCoordinateTransformInfo> GetTransformReverseReferenced(int targetCode) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgCoordinateTransformInfo>>() != null);
            throw new NotImplementedException();
            //return ConvertToOperations(TransformLookUp.GetReverseReferencedOperationCodes(targetCode));
        }

        /*
        [Obsolete]
        private static ReadOnlyCollection<EpsgCoordinateTransformInfo> ConvertToOperations(ushort[] ids) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgCoordinateTransformInfo>>() != null);
            return null == ids ? EmptyEcti : Array.AsReadOnly(Array.ConvertAll(ids, TransformLookUp.Get));
        }*/

        [Obsolete]
        public static ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo> GetConcatenatedForwardReferenced(int sourceCode) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo>>() != null);
            throw new NotImplementedException();
            //return ConvertToCatOperations(ConcatenatedLookUp.GetForwardReferencedOperationCodes(sourceCode));
        }

        [Obsolete]
        public static ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo> GetConcatenatedReverseReferenced(int targetCode) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo>>() != null);
            throw new NotImplementedException();
            //return ConvertToCatOperations(ConcatenatedLookUp.GetReverseReferencedOperationCodes(targetCode));
        }

        /*
        [Obsolete]
        private static ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo> ConvertToCatOperations(ushort[] ids) {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<EpsgConcatenatedCoordinateOperationInfo>>() != null);
            return null == ids ? EmptyEccoi : Array.AsReadOnly(Array.ConvertAll(ids, ConcatenatedLookUp.Get));
        }*/
    }
}
