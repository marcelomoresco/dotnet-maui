using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ViewHandlerTests
	{
		[Fact]
		public async Task ChildIsVisibleIfWrapperIsVisible()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var child = new Button(MauiContext.Context);
				child.Visibility = ViewStates.Gone;

				var wrapper = new WrapperView(MauiContext.Context);
				wrapper.Visibility = ViewStates.Gone;
				wrapper.AddView(child);

				Assert.Equal(ViewStates.Gone, wrapper.Visibility);
				Assert.Equal(ViewStates.Gone, child.Visibility);

				wrapper.Visibility = ViewStates.Visible;

				Assert.Equal(ViewStates.Visible, wrapper.Visibility);
				Assert.Equal(ViewStates.Visible, child.Visibility);
			});
		}

		[Fact]
		public async Task ShadowInvalidationOnlyFollowsImmediateChild()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var wrapper = new WrapperView(MauiContext.Context);
				var child = new AView(MauiContext.Context);
				var descendant = new AView(MauiContext.Context);

				wrapper.SetSolidShadow(10, 0, 0, AColor.Black.ToArgb());

				Assert.True(wrapper.ShouldInvalidateShadow(child, child));
				Assert.False(wrapper.ShouldInvalidateShadow(child, descendant));

				wrapper.SetNoShadow();

				Assert.False(wrapper.ShouldInvalidateShadow(child, child));
			});
		}

		[Theory]
		// Position and size in device pixels, both fractional, so converting
		// the left and right edges separately would lose a pixel.
		[InlineData(26.25, 1027.5)]
		[InlineData(0.5, 100.25)]
		[InlineData(10.125, 50.75)]
		public async Task PlatformArrangeKeepsFrameSizeInPixels(double positionPx, double sizePx)
		{
			var view = new StubBase();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = new StubBaseHandler();
				InitializeViewHandler(view, handler);

				var context = MauiContext.Context;
				var density = context.GetDisplayDensity();
				var frame = new Rect(positionPx / density, positionPx / density, sizePx / density, sizePx / density);

				handler.PlatformArrange(frame);

				// Measure converts a size with ToPixels, so arrange must produce the same pixel size
				var platformView = handler.PlatformView;
				Assert.Equal((int)context.ToPixels(frame.X), platformView.Left);
				Assert.Equal((int)context.ToPixels(frame.Y), platformView.Top);
				Assert.Equal((int)context.ToPixels(frame.Width), platformView.Width);
				Assert.Equal((int)context.ToPixels(frame.Height), platformView.Height);
			});
		}
	}
}