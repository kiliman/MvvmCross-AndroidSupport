// MvxAppCompatRadioGroup.cs

// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
//
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

namespace MvvmCross.Droid.Support.V7.AppCompat.Widget
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Threading;

    using Android.Content;
    using Android.Runtime;
    using Android.Support.V7.Widget;
    using Android.Util;
    using Android.Widget;

    using MvvmCross.Binding;
    using MvvmCross.Binding.Attributes;
    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Binding.Droid.Views;

    [Register("MvvmCross.Droid.Support.V7.AppCompat.widget.MvxAppCompatRadioGroup")]
    public class MvxAppCompatRadioGroup : RadioGroup, IMvxWithChangeAdapter
    {
        public MvxAppCompatRadioGroup(Context context, IAttributeSet attrs)
            : this(context, attrs, new MvxAdapterWithChangedEvent(context))
        {
        }

        public MvxAppCompatRadioGroup(Context context, IAttributeSet attrs, IMvxAdapterWithChangedEvent adapter)
            : base(context, attrs)
        {
            var itemTemplateId = MvxAttributeHelpers.ReadListItemTemplateId(context, attrs);
            if (adapter != null)
            {
                this.Adapter = adapter;
                this.Adapter.ItemTemplateId = itemTemplateId;
            }

            this.ChildViewAdded += this.OnChildViewAdded;
            this.ChildViewRemoved += this.OnChildViewRemoved;
        }

        protected MvxAppCompatRadioGroup(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public void AdapterOnDataSetChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.UpdateDataSetFromChange(sender, eventArgs);
        }

        private void OnChildViewAdded(object sender, Android.Views.ViewGroup.ChildViewAddedEventArgs args)
        {
            var li = (args.Child as MvxListItemView);
            var radioButton = li?.GetChildAt(0) as AppCompatRadioButton;
            if (radioButton != null)
            {
                // radio buttons require an id so that they get un-checked correctly
                if (radioButton.Id == Android.Views.View.NoId)
                {
                    radioButton.Id = GenerateViewId();
                }
                radioButton.CheckedChange += this.OnRadioButtonCheckedChange;
            }
        }

        private void OnRadioButtonCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs args)
        {
            var radionButton = (sender as AppCompatRadioButton);
            if (radionButton != null)
            {
                this.Check(radionButton.Id);
            }
        }

        private void OnChildViewRemoved(object sender, ChildViewRemovedEventArgs childViewRemovedEventArgs)
        {
            var boundChild = childViewRemovedEventArgs.Child as IMvxBindingContextOwner;
            boundChild?.ClearAllBindings();
        }

        private IMvxAdapterWithChangedEvent _adapter;

        public IMvxAdapterWithChangedEvent Adapter
        {
            get { return this._adapter; }
            protected set
            {
                var existing = this._adapter;
                if (existing == value)
                {
                    return;
                }

                if (existing != null)
                {
                    existing.DataSetChanged -= this.AdapterOnDataSetChanged;
                    if (value != null)
                    {
                        value.ItemsSource = existing.ItemsSource;
                        value.ItemTemplateId = existing.ItemTemplateId;
                    }
                }

                this._adapter = value;

                if (this._adapter != null)
                {
                    this._adapter.DataSetChanged += this.AdapterOnDataSetChanged;
                }

                if (this._adapter == null)
                {
                    MvxBindingTrace.Warning(
                        "Setting Adapter to null is not recommended - you may lose ItemsSource binding when doing this");
                }
            }
        }

        [MvxSetToNullAfterBinding]
        public IEnumerable ItemsSource
        {
            get { return this.Adapter.ItemsSource; }
            set { this.Adapter.ItemsSource = value; }
        }

        public int ItemTemplateId
        {
            get { return this.Adapter.ItemTemplateId; }
            set { this.Adapter.ItemTemplateId = value; }
        }

        private static long _nextGeneratedViewId = 1;

        private static int GenerateViewId()
        {
            for (;;)
            {
                int result = (int)Interlocked.Read(ref _nextGeneratedViewId);

                // aapt-generated IDs have the high byte nonzero; clamp to the range under that.
                int newValue = result + 1;
                if (newValue > 0x00FFFFFF)
                {
                    // Roll over to 1, not 0.
                    newValue = 1;
                }

                if (Interlocked.CompareExchange(ref _nextGeneratedViewId, newValue, result) == result)
                {
                    return result;
                }
            }
        }
    }
}