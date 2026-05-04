/// Этот класс отвечает за то, чтобы какая-то константа, указанная в процессе выбора папки сохранения всех записей
/// была обработана как директория. То есть здесь должно быть поле пути, потом конструктор, потому что
/// зачастую у нас будет новая папка, типовая, например "D:\MyFiles" - в результате обработки такого пути там будет
/// создана папка "SalvageNotes", например (D:\MyFiles\SalvageNotes)

package com.kilovoid.salvager;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;

public class NoteRepo {
    private final Path repoPath;

    public NoteRepo(String selectedPath) {
        Path basePath = Path.of(selectedPath); /// Вот про это я писал - то есть это выбранная папка - условная MyFiles
        this.repoPath = basePath.resolve("SalvageNotes"); /// .resolve() здесь будет присоединять к basePath дефолтный путь SalvageNotes
        try {
            Files.createDirectories(repoPath);
        } catch (IOException e) {
            throw new RuntimeException("Can't create a new notes repo!", e);
        }
    }

    public Path getRepoPath() {
        return repoPath;
    }

    private Path pathOf(String id) {
        return repoPath.resolve(id + ".txt");
    }

    public void deleteNote(String id) throws IOException {
        Files.delete(repoPath.resolve(id + ".txt"));
    }

    private void writeToFile(Note note) throws  IOException {
        Path file = repoPath.resolve(note.getId()+".txt");
        String content = note.getTitle() + "\n\n" + note.getContent();
        Files.writeString(file, content);
    }

    public void saveNote(Note note) throws IOException {
        if (note.getId() == null) {
            while (true) {
                String noteId = IdGen.idGen();
                if (!(Files.exists(pathOf(noteId)))) {
                    note.setId(noteId);
                    break;
                }
            }
        } else if (!(Files.exists(pathOf(note.getId())))) {
            throw new RuntimeException("Note " + note.getId() + " does not exist");
        }
        writeToFile(note);
    }
}
