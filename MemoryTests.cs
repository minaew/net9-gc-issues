using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NUnit.Framework;

namespace Sample {
    [TestFixture]
    public class MemoryTests {
        [Test, RequiresThread(ApartmentState.STA)]
        public void TestLeak() {
            PresentationTraceSources.Refresh();
            var window = new Window();
            var wr = AddAndRemoveImage(window);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr.IsAlive);
        }

        static WeakReference AddAndRemoveImage(Window window) {
            var img = new Image();
            var wr = new WeakReference(img);
            var holder = new ImageHolder(img);
            window.Content = img;
            window.Show();
            DoEvents();
            window.Content = null;
            DoEvents();
            return wr;
        }

        static void DoEvents() {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.InvokeAsync(() => frame.Continue = false, DispatcherPriority.Background);
            Dispatcher.PushFrame(frame);
        }

        class ImageHolder {
            readonly Image image;
            public ImageHolder(Image image) {
                this.image = image;
                this.image.Loaded += OnLoaded;
                this.image.Unloaded += OnUnloaded;
            }

            void OnLoaded(object sender, RoutedEventArgs e) {
                Debug.WriteLine("OnLoaded");
            }

            void OnUnloaded(object sender, RoutedEventArgs e) {
                Debug.WriteLine("OnUnloaded");
            }
        }
    }
}