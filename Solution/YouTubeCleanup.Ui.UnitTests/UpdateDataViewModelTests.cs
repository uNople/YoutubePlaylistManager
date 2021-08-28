using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace YouTubeCleanup.Ui.UnitTests
{
    public class UpdateDataViewModelTests
    {
        [Theory, AutoNSubstituteData]
        public async Task When_viewmodel_hidden_Then_window_gets_hidden(
            [Frozen] IUpdateDataWindow updateDataWindow,
            UpdateDataViewModel updateDataViewModel)
        {
            await updateDataViewModel.Hide();
            updateDataWindow.Received(1).Hide();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_task_run_then_completed_Then_active_jobs_should_be_empty(
            UpdateDataViewModel updateDataViewModel,
            Guid taskGuid,
            string title,
            CancellationTokenSource cancellationTokenSource
            )
        {
            await updateDataViewModel.CreateNewActiveTask(taskGuid, title, cancellationTokenSource);
            updateDataViewModel.ActiveJobs.ContainsKey(taskGuid).Should().BeTrue();
            await updateDataViewModel.SetActiveTaskComplete(taskGuid, title);
            updateDataViewModel.ActiveJobs.ContainsKey(taskGuid).Should().BeFalse();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_task_run_then_tasks_cancelled_Then_active_jobs_should_be_cancelled(
            UpdateDataViewModel updateDataViewModel,
            List<Guid> taskGuids,
            string title,
            CancellationTokenSource cancellationTokenSource
        )
        {
            foreach (var guid in taskGuids)
            {
                await updateDataViewModel.CreateNewActiveTask(guid, title, cancellationTokenSource);
                updateDataViewModel.ActiveJobs.ContainsKey(guid).Should().BeTrue();
            }

            updateDataViewModel.ActiveJobs.Count.Should().Be(taskGuids.Count());
            await updateDataViewModel.CancelActiveTasks();
            updateDataViewModel.ActiveJobs.Should().BeEmpty();
            cancellationTokenSource.IsCancellationRequested.Should().BeTrue();
        }
    }
}
