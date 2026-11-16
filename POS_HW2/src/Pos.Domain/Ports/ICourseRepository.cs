using Pos.Domain.Entities;

namespace Pos.Domain.Ports;

// Порт: домен не знает, хранится курс в JSON, БД или памяти.
public interface ICourseRepository
{
    Course Load();
    void Save(Course course);
}
