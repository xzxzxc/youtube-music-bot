﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YoutubeMusicBot.Infrastructure.Database;

namespace YoutubeMusicBot.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20210829074940_Fix_Event_Discriminator")]
    partial class Fix_Event_Discriminator
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("YoutubeMusicBot.Domain.Base.EventBase<YoutubeMusicBot.Domain.Message>", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<long>("AggregateId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("event_type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AggregateId");

                    b.HasIndex("event_type");

                    b.ToTable("EventBase<Message>");

                    b.HasDiscriminator<int>("event_type");
                });

            modelBuilder.Entity("YoutubeMusicBot.Domain.FileToBeSentCreatedEvent", b =>
                {
                    b.HasBaseType("YoutubeMusicBot.Domain.Base.EventBase<YoutubeMusicBot.Domain.Message>");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(-1052586310);
                });

            modelBuilder.Entity("YoutubeMusicBot.Domain.LoadingProcessMessageSentEvent", b =>
                {
                    b.HasBaseType("YoutubeMusicBot.Domain.Base.EventBase<YoutubeMusicBot.Domain.Message>");

                    b.Property<int>("MessageId")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue(-1727421076);
                });

            modelBuilder.Entity("YoutubeMusicBot.Domain.MessageCreatedEvent", b =>
                {
                    b.HasBaseType("YoutubeMusicBot.Domain.Base.EventBase<YoutubeMusicBot.Domain.Message>");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExternalId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(-724846401);
                });

            modelBuilder.Entity("YoutubeMusicBot.Domain.MessageFinishedEvent", b =>
                {
                    b.HasBaseType("YoutubeMusicBot.Domain.Base.EventBase<YoutubeMusicBot.Domain.Message>");

                    b.HasDiscriminator().HasValue(-1987685087);
                });

            modelBuilder.Entity("YoutubeMusicBot.Domain.MessageInvalidEvent", b =>
                {
                    b.HasBaseType("YoutubeMusicBot.Domain.Base.EventBase<YoutubeMusicBot.Domain.Message>");

                    b.Property<string>("ValidationMessage")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(-795112364);
                });

            modelBuilder.Entity("YoutubeMusicBot.Domain.MessageValidEvent", b =>
                {
                    b.HasBaseType("YoutubeMusicBot.Domain.Base.EventBase<YoutubeMusicBot.Domain.Message>");

                    b.HasDiscriminator().HasValue(-2141463875);
                });

            modelBuilder.Entity("YoutubeMusicBot.Domain.MusicFileCreatedEvent", b =>
                {
                    b.HasBaseType("YoutubeMusicBot.Domain.Base.EventBase<YoutubeMusicBot.Domain.Message>");

                    b.Property<string>("DescriptionFilePath")
                        .HasColumnType("TEXT");

                    b.Property<string>("MusicFilePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(857331207);
                });
#pragma warning restore 612, 618
        }
    }
}