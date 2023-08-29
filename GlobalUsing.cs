global using System.Security.Cryptography;
global using System.Text;
global using System.Security.Claims;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using AutoMapper;

global using MyJob.Data;
global using MyJob.DTOs;
global using MyJob.Interfaces;
global using MyJob.Entities;
global using MyJob.Extensions;

//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;


namespace MyJob;
public static class Globals
{
    public static string GmailCode { get; set; }
}