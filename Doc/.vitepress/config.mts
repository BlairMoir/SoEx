import { defineConfig } from 'vitepress'
import d2 from "vitepress-plugin-d2"
import { Layout, Theme, FileType } from 'vitepress-plugin-d2/dist/config';

// https://vitepress.dev/reference/site-config
export default defineConfig({
  base: "/SoEx/",
  title: "SoEx",
  head: [['link', { rel: 'icon', href: '/SoEx/SoExIcon.svg'}]],
  description: "SoEx Documentation",
  ignoreDeadLinks: true,
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    logo: '/SoExIcon.svg',
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Documentation', link: '/tutorial' }
    ],

    sidebar: [
      {
        text: 'Documentation',
        items: [
          { text: 'Tutorial', link: '/tutorial' },
          { text: 'How To', link: '/howto' },
          { text: 'Reference', link: '/reference' },
          { text: 'Explanation', link: '/explanation' },
          { text: 'Protection', link: '/protection' }
        ]
      }
    ],
  },
  markdown: {
    config: (md) => {
      md.use(d2, {
        forceAppendix: false,
        layout: Layout.TALA,
        theme: Theme.NEUTRAL_DEFAULT,
        darkTheme: Theme.DARK_MUAVE,
        padding: 100,
        animatedInterval: 0,
        timeout: 120,
        sketch: false,
        center: false,
        scale: -1,
        target: "*",
        fontItalic: null,
        fontBold: null,
        fontSemiBold: null,
        fileType: FileType.SVG,
        directory: "d2-diagrams",
      });
    },
  },
  
})
