// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Collections
{
    public static class IJobsLegacyExtensions
    {
#if !UNITY_2022_2_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RunByRef<T>(ref this T jobData) where T : struct, IJob
        {
            JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf<T>(ref jobData), IJobExtensions.JobStruct<T>.jobReflectionData, new JobHandle(), ScheduleMode.Run);
            JobsUtility.Schedule(ref parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JobHandle ScheduleByRef<T>(ref this T jobData, JobHandle dependsOn = default) where T : struct, IJob
        {
            JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf<T>(ref jobData), IJobExtensions.JobStruct<T>.jobReflectionData, dependsOn, ScheduleMode.Single);
            return JobsUtility.Schedule(ref parameters);
        }
#endif
    }
}
