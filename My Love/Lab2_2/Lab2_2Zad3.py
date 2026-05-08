import tkinter as tk
from tkinter import messagebox, ttk


PUNCT = {",", ".", "?", "!", "-", ":", "'"}


def main():
    root = tk.Tk()
    root.title("ЛР 2_2, вариант 8 — Задание 3")
    root.geometry("940x650")

    ttk.Label(
        root,
        text="Задание 3.\n"
        "Форматирование текста по ширине w.\n"
        "Абзацы разделены пустыми строками.\n"
        "Первая строка абзаца начинается с b пробелов.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    top = ttk.Frame(root)
    top.pack(fill="x", padx=10, pady=6)
    ttk.Label(top, text="w:").pack(side="left")
    ent_w = ttk.Entry(top, width=8)
    ent_w.pack(side="left", padx=(4, 12))
    ent_w.insert(0, "40")

    ttk.Label(top, text="b:").pack(side="left")
    ent_b = ttk.Entry(top, width=8)
    ent_b.pack(side="left", padx=(4, 12))
    ent_b.insert(0, "4")

    punct_mode = tk.StringVar(value="merge")  # radiobutton
    opts = ttk.LabelFrame(top, text="Пунктуация")
    opts.pack(side="left", padx=8)
    ttk.Radiobutton(opts, text="к слову", variable=punct_mode, value="merge").pack(
        side="left", padx=8, pady=4
    )
    ttk.Radiobutton(opts, text="отдельно", variable=punct_mode, value="sep").pack(
        side="left", padx=8, pady=4
    )

    btn = ttk.Button(top, text="Ок", command=lambda: run())
    btn.pack(side="right")

    pan = ttk.Panedwindow(root, orient="horizontal")
    pan.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    left = ttk.Frame(pan)
    right = ttk.Frame(pan)
    pan.add(left, weight=1)
    pan.add(right, weight=1)

    ttk.Label(left, text="Ввод:").pack(anchor="w")
    inp = tk.Text(left, wrap="word")
    inp.pack(fill="both", expand=True, pady=(4, 0))

    ttk.Label(right, text="Вывод:").pack(anchor="w")
    out = tk.Text(right, wrap="none")
    out.pack(fill="both", expand=True, pady=(4, 0))

    def split_paragraphs(text: str):
        text = text.replace("\r\n", "\n").replace("\r", "\n")
        lines = text.split("\n")
        pars = []
        cur = []
        for line in lines:
            if line.strip() == "":
                if cur:
                    pars.append(" ".join(cur).strip())
                    cur = []
            else:
                cur.append(line.strip())
        if cur:
            pars.append(" ".join(cur).strip())
        return pars

    def tokenize(p: str):
        tok = []
        cur = ""
        for ch in p:
            if ch.isspace():
                if cur != "":
                    tok.append(cur)
                    cur = ""
                continue
            if ch in PUNCT:
                if cur != "":
                    tok.append(cur)
                    cur = ""
                tok.append(ch)
            else:
                cur += ch
        if cur != "":
            tok.append(cur)
        return tok

    def merge_punct(tok):
        res = []
        for t in tok:
            if t in PUNCT and res:
                res[-1] = res[-1] + t
            else:
                res.append(t)
        return res

    def wrap(items, w, b):
        lines = []
        line = " " * b
        first = True
        for it in items:
            if line.strip() == "":
                cand = line + it
            else:
                cand = line + " " + it

            if len(cand) <= w:
                line = cand
            else:
                if line.strip() != "":
                    lines.append(line.rstrip())
                if first:
                    first = False
                line = it
        if line.strip() != "":
            lines.append(line.rstrip())
        return lines

    def run():
        try:
            w = int(ent_w.get().strip())
            b = int(ent_b.get().strip())
        except Exception:
            messagebox.showerror("Ошибка", "w и b должны быть числами.")
            return
        if w <= 0 or b < 0:
            messagebox.showerror("Ошибка", "w > 0, b >= 0")
            return

        text = inp.get("1.0", "end-1c")
        pars = split_paragraphs(text)
        result = []
        for p in pars:
            tok = tokenize(p)
            if punct_mode.get() == "merge":
                items = merge_punct(tok)
            else:
                items = tok
            lines = wrap(items, w, b)
            result.append("\n".join(lines))
        out_text = "\n".join(result)  # без пустых строк между абзацами

        out.delete("1.0", "end")
        out.insert("1.0", out_text)

    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()

