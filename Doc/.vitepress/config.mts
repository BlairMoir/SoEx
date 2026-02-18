import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  base: "/SoEx/",
  title: "SoEx",
  description: "SoEx Documentation",
  ignoreDeadLinks: true,
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    logo: '/SoEx/images/SoExIcon.svg',
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
          { text: 'Explanation', link: '/explanation' }

        ]
      }
    ],
  }
})
