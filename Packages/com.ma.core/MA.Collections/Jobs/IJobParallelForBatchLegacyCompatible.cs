// Copyright © Magnetic Arcade. All Rights Reserved.

namespace Unity.Collections
{
    using Unity.Jobs;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Unity.Burst;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Jobs.LowLevel.Unsafe;

    [JobProducerType(typeof(IJobParallelForBatchLegacyCompatibleExtensions.JobParallelForBatchLegacyProducer<>))]
    public interface IJobParallelForBatchLegacyCompatible 
    {
        /// <summary>
        /// Function operation on a "batch" of data contained within the job.
        /// </summary>
        /// <param name="startIndex">Starting index of job data to safely access.</param>
        /// <param name="count">Number of elements to operate on in the batch.</param>
        void Execute(int startIndex, int count);
    }

    public static unsafe class IJobParallelForBatchLegacyCompatibleExtensions
    {
        internal struct JobParallelForBatchLegacyProducer<T> where T : struct, IJobParallelForBatchLegacyCompatible
        {
            internal static readonly SharedStatic<IntPtr> jobReflectionData = SharedStatic<IntPtr>.GetOrCreate<JobParallelForBatchLegacyProducer<T>>();

            [BurstDiscard]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Initialize()
            {
                if (jobReflectionData.Data == IntPtr.Zero)
                    jobReflectionData.Data = JobsUtility.CreateJobReflectionData(typeof(T), (ExecuteJobFunction)Execute);
            }

            internal delegate void ExecuteJobFunction(ref T jobData, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Execute(ref T jobData, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
            {
                while (true)
                {
                    if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out int begin, out int end))
                        return;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData, UnsafeUtility.AddressOf(ref jobData), begin, end - begin);
#endif

                    jobData.Execute(begin, end - begin);
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EarlyJobInit<T>()
            where T : struct, IJobParallelForBatchLegacyCompatible
        {
            JobParallelForBatchLegacyProducer<T>.Initialize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IntPtr GetReflectionData<T>()
            where T : struct, IJobParallelForBatchLegacyCompatible
        {
            JobParallelForBatchLegacyProducer<T>.Initialize();
            var reflectionData = JobParallelForBatchLegacyProducer<T>.jobReflectionData.Data;
            CheckReflectionDataCorrect(reflectionData);
            return reflectionData;
        }
        
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CheckReflectionDataCorrect(IntPtr reflectionData)
        {
            if (reflectionData == IntPtr.Zero)
                throw new InvalidOperationException("Reflection data was not set up by an Initialize() call");
        }
        
        /// <summary>
        /// Schedules a job that will execute the parallel batch job for all `arrayLength` elements in batches of `indicesPerJobCount`.
        /// The Execute() method for Job T will be provided the start index and number of elements to safely operate on.
        /// In cases where `indicesPerJobCount` is not a multiple of `arrayLength`, the `count` provided to the Execute method of Job T will be smaller than the `indicesPerJobCount` specified here.
        /// </summary>
        /// <param name="jobData">The job and data to schedule.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <param name="indicesPerJobCount">Number of elements to consider in a single parallel batch.</param>
        /// <param name="dependsOn">Dependencies are used to ensure that a job executes on workerthreads after the dependency has completed execution. Making sure that two jobs reading or writing to same data do not run in parallel.</param>
        /// <returns>JobHandle The handle identifying the scheduled job. Can be used as a dependency for a later job or ensure completion on the main thread.</returns>
        /// <typeparam name="T">Job type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle Schedule<T>(this T jobData, int arrayLength, int indicesPerJobCount,
            JobHandle dependsOn = new JobHandle()) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), dependsOn, ScheduleMode.Single);
            return JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, indicesPerJobCount);
        }

        /// <summary>
        /// Schedules a job that will execute the parallel batch job for all `arrayLength` elements in batches of `indicesPerJobCount`.
        /// The Execute() method for Job T will be provided the start index and number of elements to safely operate on.
        /// In cases where `indicesPerJobCount` is not a multiple of `arrayLength`, the `count` provided to the Execute method of Job T will be smaller than the `indicesPerJobCount` specified here.
        /// </summary>
        /// <param name="jobData">The job and data to schedule. In this variant, the jobData is
        /// passed by reference, which may be necessary for unusually large job structs.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <param name="indicesPerJobCount">Number of elements to consider in a single parallel batch.</param>
        /// <param name="dependsOn">Dependencies are used to ensure that a job executes on workerthreads after the dependency has completed execution. Making sure that two jobs reading or writing to same data do not run in parallel.</param>
        /// <returns>JobHandle The handle identifying the scheduled job. Can be used as a dependency for a later job or ensure completion on the main thread.</returns>
        /// <typeparam name="T">Job type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleByRef<T>(this ref T jobData, int arrayLength, int indicesPerJobCount,
            JobHandle dependsOn = new JobHandle()) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), dependsOn, ScheduleMode.Single);
            return JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, indicesPerJobCount);
        }

        /// <summary>
        /// Schedules a job that will execute the parallel batch job for all `arrayLength` elements in batches of `indicesPerJobCount`.
        /// The Execute() method for Job T will be provided the start index and number of elements to safely operate on.
        /// In cases where `indicesPerJobCount` is not a multiple of `arrayLength`, the `count` provided to the Execute method of Job T will be smaller than the `indicesPerJobCount` specified here.
        /// </summary>
        /// <param name="jobData">The job and data to schedule.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <param name="indicesPerJobCount">Number of elements to consider in a single parallel batch.</param>
        /// <param name="dependsOn">Dependencies are used to ensure that a job executes on workerthreads after the dependency has completed execution. Making sure that two jobs reading or writing to same data do not run in parallel.</param>
        /// <returns>JobHandle The handle identifying the scheduled job. Can be used as a dependency for a later job or ensure completion on the main thread.</returns>
        /// <typeparam name="T">Job type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleParallel<T>(this T jobData, int arrayLength, int indicesPerJobCount,
            JobHandle dependsOn = new JobHandle()) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), dependsOn, ScheduleMode.Parallel);
            return JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, indicesPerJobCount);
        }

        /// <summary>
        /// Schedules a job that will execute the parallel batch job for all `arrayLength` elements in batches of `indicesPerJobCount`.
        /// The Execute() method for Job T will be provided the start index and number of elements to safely operate on.
        /// In cases where `indicesPerJobCount` is not a multiple of `arrayLength`, the `count` provided to the Execute method of Job T will be smaller than the `indicesPerJobCount` specified here.
        /// </summary>
        /// <param name="jobData">The job and data to schedule. In this variant, the jobData is
        /// passed by reference, which may be necessary for unusually large job structs.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <param name="indicesPerJobCount">Number of elements to consider in a single parallel batch.</param>
        /// <param name="dependsOn">Dependencies are used to ensure that a job executes on workerthreads after the dependency has completed execution. Making sure that two jobs reading or writing to same data do not run in parallel.</param>
        /// <returns>JobHandle The handle identifying the scheduled job. Can be used as a dependency for a later job or ensure completion on the main thread.</returns>
        /// <typeparam name="T">Job type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleParallelByRef<T>(this ref T jobData, int arrayLength, int indicesPerJobCount,
            JobHandle dependsOn = new JobHandle()) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), dependsOn, ScheduleMode.Parallel);
            return JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, indicesPerJobCount);
        }

        /// <summary>
        /// Schedules a job that will execute the parallel batch job for all `arrayLength` elements in batches of `indicesPerJobCount`.
        /// The Execute() method for Job T will be provided the start index and number of elements to safely operate on.
        /// In cases where `indicesPerJobCount` is not a multiple of `arrayLength`, the `count` provided to the Execute method of Job T will be smaller than the `indicesPerJobCount` specified here.
        /// </summary>
        /// <param name="jobData">The job and data to schedule.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <param name="indicesPerJobCount">Number of elements to consider in a single parallel batch.</param>
        /// <param name="dependsOn">Dependencies are used to ensure that a job executes on workerthreads after the dependency has completed execution. Making sure that two jobs reading or writing to same data do not run in parallel.</param>
        /// <returns>JobHandle The handle identifying the scheduled job. Can be used as a dependency for a later job or ensure completion on the main thread.</returns>
        /// <typeparam name="T">Job type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleBatch<T>(this T jobData, int arrayLength, int indicesPerJobCount,
            JobHandle dependsOn = new JobHandle()) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            return ScheduleParallel(jobData, arrayLength, indicesPerJobCount, dependsOn);
        }

        /// <summary>
        /// Schedules a job that will execute the parallel batch job for all `arrayLength` elements in batches of `indicesPerJobCount`.
        /// The Execute() method for Job T will be provided the start index and number of elements to safely operate on.
        /// In cases where `indicesPerJobCount` is not a multiple of `arrayLength`, the `count` provided to the Execute method of Job T will be smaller than the `indicesPerJobCount` specified here.
        /// </summary>
        /// <param name="jobData">The job and data to schedule. In this variant, the jobData is
        /// passed by reference, which may be necessary for unusually large job structs.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <param name="indicesPerJobCount">Number of elements to consider in a single parallel batch.</param>
        /// <param name="dependsOn">Dependencies are used to ensure that a job executes on workerthreads after the dependency has completed execution. Making sure that two jobs reading or writing to same data do not run in parallel.</param>
        /// <returns>JobHandle The handle identifying the scheduled job. Can be used as a dependency for a later job or ensure completion on the main thread.</returns>
        /// <typeparam name="T">Job type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleBatchByRef<T>(this ref T jobData, int arrayLength, int indicesPerJobCount,
            JobHandle dependsOn = new JobHandle()) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            return ScheduleParallelByRef(ref jobData, arrayLength, indicesPerJobCount, dependsOn);
        }

        /// <summary>
        /// Executes the parallel batch job but on the main thread. See IJobParallelForBatchExtensions.Schedule for more information on how appending is performed.
        /// </summary>
        /// <param name="jobData">The job and data to schedule.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <param name="indicesPerJobCount">Number of elements to consider in a single parallel batch. This argument is ignored when using .Run()</param>
        /// <typeparam name="T">Job type</typeparam>
        /// <remarks>
        /// Unlike Schedule, since the job is running on the main thread no parallelization occurs and thus no `indicesPerJobCount` batch size is required to be specified.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run<T>(this T jobData, int arrayLength, int indicesPerJobCount) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), new JobHandle(), ScheduleMode.Run);
            JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, arrayLength);
        }

        /// <summary>
        /// Executes the parallel batch job but on the main thread. See IJobParallelForBatchExtensions.Schedule for more information on how appending is performed.
        /// </summary>
        /// <param name="jobData">The job and data to schedule. In this variant, the jobData is
        /// passed by reference, which may be necessary for unusually large job structs.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <param name="indicesPerJobCount">Number of elements to consider in a single parallel batch. This argument is ignored when using .RunByRef()</param>
        /// <typeparam name="T">Job type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunByRef<T>(this ref T jobData, int arrayLength, int indicesPerJobCount) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), new JobHandle(), ScheduleMode.Run);
            JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, arrayLength);
        }

        /// <summary>
        /// Executes the parallel batch job but on the main thread. See IJobParallelForBatchExtensions.ScheduleBatch for more information on how appending is performed.
        /// </summary>
        /// <param name="jobData">The job and data to schedule.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <typeparam name="T">Job type</typeparam>
        /// <remarks>
        /// Unlike ScheduleBatch, since the job is running on the main thread no parallelization occurs and thus no `indicesPerJobCount` batch size is required to be specified.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunBatch<T>(this T jobData, int arrayLength) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            Run(jobData, arrayLength, arrayLength);
        }

        /// <summary>
        /// Executes the parallel batch job but on the main thread. See IJobParallelForBatchExtensions.ScheduleBatch for more information on how appending is performed.
        /// </summary>
        /// <param name="jobData">The job and data to schedule. In this variant, the jobData is
        /// passed by reference, which may be necessary for unusually large job structs.</param>
        /// <param name="arrayLength">Total number of elements to consider when batching.</param>
        /// <typeparam name="T">Job type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunBatchByRef<T>(this ref T jobData, int arrayLength) where T : struct, IJobParallelForBatchLegacyCompatible
        {
            RunByRef(ref jobData, arrayLength, arrayLength);
        }
    }
}