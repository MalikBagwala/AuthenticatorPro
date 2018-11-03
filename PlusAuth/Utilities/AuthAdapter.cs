﻿using System;
using System.Collections.Generic;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using OtpSharp;
using PlusAuth.Data;

namespace PlusAuth.Utilities
{
    internal sealed class AuthAdapter : RecyclerView.Adapter
    {
        private readonly AuthSource _authSource;
        public event EventHandler<int> ItemClick;
        public event EventHandler<int> ItemOptionsClick;

        public AuthAdapter(AuthSource authSource)
        {
            _authSource = authSource;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            Authenticator auth = _authSource.Get(position);
            AuthHolder holder = (AuthHolder) viewHolder;

            holder.Issuer.Text = auth.Issuer;
            holder.Username.Text = auth.Username;

            holder.Username.Visibility = auth.Username == "" 
                ? ViewStates.Gone 
                : ViewStates.Visible;

            string codePadded = auth.Code;
            int spacesInserted = 0;
            int length = codePadded.Length;

            for(int i = 0; i < length; ++i)
            {
                if(i % 3 == 0 && i > 0)
                {
                    codePadded = codePadded.Insert(i + spacesInserted, " ");
                    spacesInserted++;
                }
            }

            if(auth.Type == OtpType.Totp)
                TotpViewBind(holder, auth);

            else if(auth.Type == OtpType.Hotp) 
                HotpViewBind(holder, auth);

            holder.Code.Text = codePadded;
        }

        private static void TotpViewBind(AuthHolder holder, Authenticator auth)
        {
            holder.RefreshButton.Visibility = ViewStates.Gone;
            holder.Timer.Visibility = ViewStates.Visible;
            holder.Counter.Visibility = ViewStates.Invisible;
            holder.Timer.Text = (auth.TimeRenew - DateTime.Now).Seconds.ToString();
        }

        private static void HotpViewBind(AuthHolder holder, Authenticator auth)
        {
            holder.RefreshButton.Visibility = (auth.TimeRenew < DateTime.Now)
                ? ViewStates.Visible
                : ViewStates.Gone;

            holder.Timer.Visibility = ViewStates.Invisible;
            holder.Counter.Visibility = ViewStates.Visible;
            holder.Counter.Text = auth.Counter.ToString();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(
                Resource.Layout.authListItem, parent, false);

            AuthHolder holder = new AuthHolder(itemView, OnItemClick, OnItemOptionsClick, OnRefreshClick);

            return holder;
        }

        public override int ItemCount => _authSource.Count();

        private void OnItemClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        private void OnItemOptionsClick(int position)
        {
            ItemOptionsClick?.Invoke(this, position);
        }

        private void OnRefreshClick(int position)
        {
            _authSource.IncrementHotp(position);
            NotifyItemChanged(position);
        }
    }
}