﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YoutubeMusicBot.Infrastructure.Database;

namespace YoutubeMusicBot.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20210823195127_Add_ExternalId_ToMessage")]
    partial class Add_ExternalId_ToMessage
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

            modelBuilder.Entity("YoutubeMusicBot.Domain.MessageCreatedEvent", b =>
                {
                    b.HasBaseType("YoutubeMusicBot.Domain.Base.EventBase<YoutubeMusicBot.Domain.Message>");

                    b.Property<int>("ExternalId")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue(732838313);
                });
#pragma warning restore 612, 618
        }
    }
}