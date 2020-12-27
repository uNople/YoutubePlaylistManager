using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YouTubeCleanupTool.Domain;

namespace YoutubeCleanupWpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IYouTubeCleanupToolDbContext _youTubeCleanupToolDbContext;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<VideoData> Videos { get; set; }
        public VideoData SelectedVideo { get; set; }

        public MainWindowViewModel([NotNull] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext)
        {
            _youTubeCleanupToolDbContext = youTubeCleanupToolDbContext;
            Videos = new ObservableCollection<VideoData>();
        }

        public async Task LoadData()
        {
            var videos = await _youTubeCleanupToolDbContext.GetVideos();
            foreach (var video in videos.Take(10))
            {
                try
                {
                    Videos.AddOnUI(video);
                }
                catch (Exception ex)
                {

                }

            }
        }
    }


    // TODO: better way?
    public static class CollectionExtensions
    {
        public static void AddOnUI<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }
    }
}
