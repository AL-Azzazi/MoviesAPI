﻿using AutoMapper;
using System.Runtime.InteropServices;

namespace MoviesAPI.Helpers
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<Movie, MovieDetailsDto>();

            CreateMap<MovieDto, Movie>()
                .ForMember(src => src.Poster, opt => opt.Ignore());
        }

        
    }
}
